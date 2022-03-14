using Microsoft.EntityFrameworkCore;

namespace azpush;

public record AddPath(string Path, string CredentialName, string ContainerUrl, string? BlobName, bool IncludeTimestamp) : ICommand { }

public class AddPathHandler : IAsyncCommandHandler<AddPath, int>
{
    private readonly IFileSystem _fs;
    private readonly SyncDbContext _context;
    private readonly IAzureCredentialRepository _credentials;


    public AddPathHandler(IFileSystem fileSystem, SyncDbContext context, IAzureCredentialRepository credentials)
    {
        _fs = fileSystem;
        _context = context;
        _credentials = credentials;
    }

    public async Task<int> Handle(AddPath command)
    {
        var credential = await _credentials.GetByNameAsync(command.CredentialName);

        if (credential is null)
        {
            Console.WriteLine($"Cannot find credental '{command.CredentialName}'.");
            return AppConstants.ERROR_EXIT_CODE;
        }

        var formattedUrl = command.ContainerUrl;

        if (command.ContainerUrl[^1] != '/')
            formattedUrl = command.ContainerUrl + "/";

        var path = _fs.CreatePath(path: command.Path, credentialId: credential.Id, containerUrl: formattedUrl, blobName: command.BlobName, includeTimestamp: command.IncludeTimestamp);

        if (path.PathType == LocalPathType.Invalid.Name)
        {
            Console.WriteLine($"{path.Path} is an invalid glob, file, or directory path.");
            return AppConstants.ERROR_EXIT_CODE;
        }

        if (_context.LocalPaths.Any(p => p.Path == path.Path) is false)
        {
            _context.LocalPaths.Add(path);
            await _context.SaveChangesAsync();
        }

        Console.WriteLine($"{path.Path} will be copied to {path.ContainerUrl}.");
        return AppConstants.OK_EXIT_CODE;
    }
}
