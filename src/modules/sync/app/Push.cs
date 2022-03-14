using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;

namespace azpush;

public record Push : ICommand { }

public class PushHandler : IAsyncCommandHandler<Push, int>
{
    private readonly IFileSystem _fileSystem;
    private readonly SyncDbContext _context;
    private readonly LocalFileInfoCache _fileInfoCache;
    private readonly IBlobFileRepository _blobs;
    private readonly IStringProtector _protector;

    public PushHandler(IFileSystem fileSystem, SyncDbContext context, LocalFileInfoCache fileInfoCache, IBlobFileRepository syncFiles, IStringProtector protector)
    {
        _fileSystem = fileSystem;
        _context = context;
        _fileInfoCache = fileInfoCache;
        _blobs = syncFiles;
        _protector = protector;
    }
    public async Task<int> Handle(Push command)
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
                return AppConstants.ERROR_EXIT_CODE;
            }

            var credentials = await _context.AzureCredentials.FirstOrDefaultAsync(c => c.Id == path.CredentialId);

            if (credentials is null)
            {
                Console.WriteLine($"Unable to find credentials.");
                return AppConstants.ERROR_EXIT_CODE;
            }
            
            var blobCredentials = new ClientSecretCredential(tenantId: credentials.Tenant, clientId: credentials.Client, clientSecret: credentials.GetSecret(_protector));
            
            await _fileInfoCache.AddAsync(fileInfos);
            await PushNewAsync(path, blobCredentials);
            await PushModifiedAsync(path, blobCredentials);
            var deletedCount = await PushDeletedAsync(path);

            if (deletedCount > 0) return AppConstants.ERROR_EXIT_CODE;
        }

        return AppConstants.OK_EXIT_CODE;
    }

    private async Task PushNewAsync(LocalPath path, ClientSecretCredential credentials)
    {
        foreach (var fileInfo in await _fileInfoCache.GetNewAsync(path.Id))
        {
            var file = new BlobFile(localFileName: fileInfo.Name, localFilePath: fileInfo.Path, localFilePathHash: fileInfo.PathHash, localPathId: fileInfo.LocalPathId, blobName: path.PathType == LocalPathType.File.Name ? path.BlobName ?? fileInfo.Name : fileInfo.Name);

            try
            {
                using var fileStream = _fileSystem.OpenFile(fileInfo.Path);

                var contentHash = await new Md5HashAlgorithm().ComputeHashAsync(fileStream);

                file.Modify(lastModified: fileInfo.LastModified, fileSizeInBytes: fileInfo.FileSizeInBytes, contentHash: contentHash);

                fileStream.Position = 0;

                var blobName = path.IncludeTimestamp ? file.GetFormattedBlobName(fileInfo.LastModified) : file.BlobName;
                var url = new Uri(path.ContainerUrl + blobName);
                var blobClient = new BlobClient(url, credentials);

                OutputFilePushMessage(state: "new", file: file, url: url.ToString());

                var blob = await blobClient.UploadAsync(fileStream, overwrite: true);

                file.Upload(blobUrl: blobClient.Uri.ToString(), blobContentHash: Convert.ToBase64String(blob.Value.ContentHash), timestamp: DateTimeOffset.Now);
            }

            catch (Exception ex)
            {
                OutputErrorMessage(ex);
                file.Error();
            }

            _blobs.Add(file);

            await _context.SaveChangesAsync();

            OutputFilePushResultMessage(file: file);
        }
    }

    private async Task<int> PushDeletedAsync(LocalPath path)
    {
        var deletedFiles = await _blobs.GetDeleted(path.Id).ToListAsync();
        var deletedCount = deletedFiles.Count;

        foreach (var file in deletedFiles)
        {
            OutputFilePushMessage(state: "deleted", file: file, url: file.BlobUrl);

            file.Delete();

            _context.BlobFiles.Remove(file);
            await _context.SaveChangesAsync();

            OutputFilePushResultMessage(file: file);
        }

        return deletedCount;
    }

    private async Task PushModifiedAsync(LocalPath path, ClientSecretCredential credentials)
    {
        var modifiedFiles = await _fileInfoCache.GetModifiedAsync(path.Id);

        foreach (var fileInfo in modifiedFiles)
        {
            var file = _context.BlobFiles.First(sf => sf.LocalFilePathHash == fileInfo.PathHash);

            try
            {
                using var fileStream = _fileSystem.OpenFile(fileInfo.Path);

                var contentHash = await new Md5HashAlgorithm().ComputeHashAsync(fileStream);

                file.Modify(lastModified: fileInfo.LastModified, fileSizeInBytes: fileInfo.FileSizeInBytes, contentHash: contentHash);

                OutputFilePushMessage(state: "modified", file: file, url: file.BlobUrl);

                fileStream.Position = 0;

                var blobName = path.IncludeTimestamp ? file.GetFormattedBlobName(fileInfo.LastModified) : file.BlobName;
                var url = new Uri(path.ContainerUrl + blobName);
                var blobClient = new BlobClient(url, credentials);
                var blob = await blobClient.UploadAsync(fileStream, overwrite: true);

                file.Upload(blobUrl: blobClient.Uri.ToString(), blobContentHash: Convert.ToBase64String(blob.Value.ContentHash), timestamp: DateTimeOffset.Now);
            }

            catch (Exception ex)
            {
                OutputErrorMessage(ex);
                file.Error();
            }

            await _context.SaveChangesAsync();

            OutputFilePushResultMessage(file: file);
        }
    }

    private static void OutputFilePushMessage(string state, BlobFile file, string url)
    {
        Console.Write($"{state}: {file.LocalFilePath} ({file.FileSizeInBytes} bytes) {url} ...");
    }

    private static void OutputFilePushResultMessage(BlobFile file)
    {
        if (file.State == "Error")
            Console.WriteLine("ERR.");

        else if (file.State == "Uploaded" || file.State == "Deleted")
            Console.WriteLine("OK.");
    }

    private static void OutputErrorMessage(Exception ex)
    {
        Console.Write("\t");
        Console.WriteLine(ex.Message);
    }
}