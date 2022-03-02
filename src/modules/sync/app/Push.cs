using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;

namespace azpush;

public record Push : ICommand { }

public class PushHandler : IAsyncCommandHandler<Push>
{
    private readonly IFileSystem _fileSystem;
    private readonly SyncDbContext _context;
    private readonly LocalFileInfoCache _fileInfoCache;
    private readonly ISyncFileRepository _syncFiles;

    public PushHandler(IFileSystem fileSystem, SyncDbContext context, LocalFileInfoCache fileInfoCache, ISyncFileRepository syncFiles)
    {
        _fileSystem = fileSystem;
        _context = context;
        _fileInfoCache = fileInfoCache;
        _syncFiles = syncFiles;
    }
    public async Task Handle(Push command)
    {
        await _fileInfoCache.PrepareAsync();

        foreach(var path in await _context.LocalPaths.ToListAsync())
        {
            Console.WriteLine($"Looking for changes to {path.PathType.ToLowerInvariant()} '{path.Path}'...");
            
            var fileInfos = Enumerable.Empty<LocalFileInfo>();

            try
            {
                // Attempt to get file information for all files in the specified path.
                fileInfos = path.GetFiles(_fileSystem);
            }

            catch (Exception)
            {
                Console.WriteLine("Unable to read from file system. Ensure this process has the correct permissions set.");
                return;
            }

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

            await _fileInfoCache.AddAsync(fileInfos);
            await PushNew(path, containerClient);
            await PushModified(path, containerClient);
            await PushDeleted(path);
        }
    }

    private async Task PushNew(LocalPath path, BlobContainerClient containerClient)
    {
        foreach (var fileInfo in await _fileInfoCache.GetNewAsync(path.Id))
        {
            var file = new RemoteFile(localFileName: fileInfo.Name, localFilePath: fileInfo.Path, localFilePathHash: fileInfo.PathHash, containerId: fileInfo.ContainerId, localPathId: fileInfo.LocalPathId, blobName: path.PathType == LocalPathType.File.Name ? path.BlobName ?? fileInfo.Name : fileInfo.Name);

            try
            {
                using var fileStream = _fileSystem.OpenFile(fileInfo.Path);

                var contentHash = await new Md5HashAlgorithm().ComputeHashAsync(fileStream);

                file.Modify(lastModified: fileInfo.LastModified, fileSizeInBytes: fileInfo.FileSizeInBytes, contentHash: contentHash);

                fileStream.Position = 0;

                var blobClient = containerClient.GetBlobClient(file.BlobName);

                OutputFilePushMessage(state: "new", file: file);

                var blob = await blobClient.UploadAsync(fileStream, overwrite: true);

                file.Upload(blobUrl: blobClient.Uri.ToString(), blobContentHash: Convert.ToBase64String(blob.Value.ContentHash), timestamp: DateTimeOffset.Now);
            }

            catch (Exception)
            {
                file.Error();
            }

            _syncFiles.Add(file);

            await _context.SaveChangesAsync();

            OutputFilePushResultMessage(file: file);
        }
    }

    private async Task PushDeleted(LocalPath path)
    {
        var deletedFiles = await _syncFiles.GetDeleted(path.Id).ToListAsync();

        foreach (var file in deletedFiles)
        {
            OutputFilePushMessage(state: "deleted", file: file);

            file.Delete();

            _context.RemoteFiles.Remove(file);
            await _context.SaveChangesAsync();

            OutputFilePushResultMessage(file: file);
        }
    }

    private async Task PushModified(LocalPath path, BlobContainerClient containerClient)
    {
        var modifiedFiles = await _fileInfoCache.GetModifiedAsync(path.Id);

        foreach (var fileInfo in modifiedFiles)
        {
            var file = _context.RemoteFiles.First(sf => sf.LocalFilePathHash == fileInfo.PathHash);

            try
            {
                using var fileStream = _fileSystem.OpenFile(fileInfo.Path);

                var contentHash = await new Md5HashAlgorithm().ComputeHashAsync(fileStream);

                file.Modify(lastModified: fileInfo.LastModified, fileSizeInBytes: fileInfo.FileSizeInBytes, contentHash: contentHash);

                OutputFilePushMessage(state: "modified", file: file);

                fileStream.Position = 0;

                var blobClient = containerClient.GetBlobClient(file.BlobName);
                var blob = await blobClient.UploadAsync(fileStream, overwrite: true);

                file.Upload(blobUrl: blobClient.Uri.ToString(), blobContentHash: Convert.ToBase64String(blob.Value.ContentHash), timestamp: DateTimeOffset.Now);
            }

            catch (Exception ex)
            {
                file.Error();
                Console.WriteLine(ex.Message);
            }

            await _context.SaveChangesAsync();

            OutputFilePushResultMessage(file: file);
        }
    }

    private static void OutputFilePushMessage(string state, RemoteFile file)
    {
        Console.Write($"{state}: {file.LocalFileName} ({file.FileSizeInBytes} bytes) {file.BlobUrl} ...");
    }

    private static void OutputFilePushResultMessage(RemoteFile file)
    {
        if (file.State == "Error")
            Console.WriteLine("ERR 😬");

        else if (file.State == "Uploaded" || file.State == "Deleted")
            Console.WriteLine("OK. 🙌");
    }
}