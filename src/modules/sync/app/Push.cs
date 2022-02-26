using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;

namespace azsync;

public record Push : ICommand { }

public class PushHandler : IAsyncCommandHandler<Push>
{
    private readonly IFileSystem _fileSystem;
    private readonly SyncDbContext _context;
    private readonly LocalFileRepository _localFiles;
    private readonly ISyncFileRepository _syncFiles;

    public PushHandler(IFileSystem fileSystem, SyncDbContext context, LocalFileRepository localFiles, ISyncFileRepository syncFiles)
    {
        _fileSystem = fileSystem;
        _context = context;
        _localFiles = localFiles;
        _syncFiles = syncFiles;
    }
    public async Task Handle(Push command)
    {
        foreach(var path in await _context.LocalPaths.ToListAsync())
        {
            Console.WriteLine($"Looking for changes to {path.PathType.ToLowerInvariant()} '{path.Path}'...");
            var localFilesPendingUpdate = Enumerable.Empty<LocalFile>();

            try
            {
                localFilesPendingUpdate = path.GetLocalFiles(_fileSystem);
            }

            catch(Exception)
            {
                Console.WriteLine("Unable to read from file system. Ensure this process has the correct permissions set.");
                return;
            }

            _localFiles.ReplaceAll(localFilesPendingUpdate);

            var container = await _context.AzureContainers.FirstAsync(c => c.Id == path.ContainerId);

            if (container is null)
            {
                Console.WriteLine($"Unable to find container '{path.ContainerId}'.");
                return;
            }

            var credentials = await _context.AzureCredentials.FirstOrDefaultAsync(c => c.Id == container.CredentialId);

            if (credentials is null)
            {
                Console.WriteLine($"Unable to find credentials.");
                return;
            }

            var serviceClient = new BlobServiceClient(new Uri(container.ContainerUrl), new ClientSecretCredential(tenantId: credentials.Tenant, clientId: credentials.Client, clientSecret: credentials.Secret));
            
            var containerClient = serviceClient.GetBlobContainerClient(container.Name);

            foreach(var fileMetadata in await _localFiles.GetNew(path.Id))
            {
                var file = new SyncFile(name: fileMetadata.Name, localFilePath: fileMetadata.Path, localFilePathHash: fileMetadata.PathHash, lastModified: fileMetadata.LastModified, fileSizeInBytes: fileMetadata.FileSizeInBytes, containerId: fileMetadata.ContainerId, localPathId: fileMetadata.LocalPathId);

                try
                {
                    using var fileStream = new FileStream(path: file.LocalFilePath, mode: FileMode.Open, access: FileAccess.Read, share: FileShare.Read, bufferSize: 4096, useAsync: true);

                    file.SetContentHash(await new Md5HashAlgorithm().ComputeHashAsync(fileStream));
                    fileStream.Seek(0, SeekOrigin.Begin);

                    Console.Write($"new: {file.Name} ({file.FileSizeInBytes} bytes) ...");

                    var blobClient = containerClient.GetBlobClient(file.Name);
                    var blob = await blobClient.UploadAsync(fileStream, overwrite: true);
                
                    file.Upload(Convert.ToBase64String(blob.Value.ContentHash), DateTimeOffset.Now);
                }

                catch(Exception)
                {
                    file.Error();
                }

                _syncFiles.Add(file);

                await _context.SaveChangesAsync();

                if (file.State == "Error")
                    Console.WriteLine("ERR 😬");

                if (file.State == "Uploaded")
                    Console.WriteLine("OK. 🙌");
            }

            var updatedFiles = await _localFiles.GetModified(path.Id).ToListAsync();

            foreach(var fileMetadata in updatedFiles)
            {
                var file = _context.SyncFiles.First(sf => sf.LocalFilePathHash == fileMetadata.PathHash);

                file.LastModified = fileMetadata.LastModified;
                file.FileSizeInBytes = fileMetadata.FileSizeInBytes;

                try
                {
                    using var fileStream = new FileStream(path: file.LocalFilePath, mode: FileMode.Open, access: FileAccess.Read, share: FileShare.Read, bufferSize: 4096, useAsync: true);

                    file.SetContentHash(await new Md5HashAlgorithm().ComputeHashAsync(fileStream));
                    fileStream.Seek(0, SeekOrigin.Begin);

                    Console.Write($"modified: {fileMetadata.Name} ({file.FileSizeInBytes} bytes) ...");

                    var blobClient = containerClient.GetBlobClient(file.Name);

                    var blob = await blobClient.UploadAsync(fileStream, overwrite: true);
                
                    file.Upload(Convert.ToBase64String(blob.Value.ContentHash), DateTimeOffset.Now);
                }

                catch(Exception ex)
                {
                    file.Error();
                    Console.WriteLine(ex.Message);
                }

                await _context.SaveChangesAsync();

                if (file.State == "Error")
                    Console.WriteLine("ERR 😬");

                if (file.State == "Uploaded")
                    Console.WriteLine("OK. 🙌");
            }
            
            var deletedFiles = await _syncFiles.GetDeleted(path.Id).ToListAsync();
            
            foreach(var file in deletedFiles)
            {
                Console.Write($"deleted: {file.Name} ({file.FileSizeInBytes} bytes) ...");

                _context.SyncFiles.Remove(file);
                await _context.SaveChangesAsync();

                Console.WriteLine("OK. 🙌");
            }
        }
    }
}