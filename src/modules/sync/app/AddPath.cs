using Microsoft.EntityFrameworkCore;

namespace azpush;

public record AddPath(string Path, string CredentialName, string ContainerUrl, string? BlobName) : ICommand { }

public class AddPathHandler : IAsyncCommandHandler<AddPath>
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

    public async Task Handle(AddPath command)
    {
        var credential = await _credentials.GetByNameAsync(command.CredentialName);

        if (credential is null)
        {
            Console.WriteLine($"Cannot find credental '{command.CredentialName}'.");
            return;
        }

        var path = _fs.CreatePath(path: command.Path, credentialId: credential.Id, containerUrl: command.ContainerUrl, blobName: command.BlobName);

        if (path.PathType == LocalPathType.Invalid.Name)
        {
            Console.WriteLine("Invalid glob, file, or directory path.");
            return;
        }

        if (_context.LocalPaths.Any(p => p.Path == path.Path) is false)
        {
            _context.LocalPaths.Add(path);
            await _context.SaveChangesAsync();
        }

        Console.WriteLine($"The {path.PathType.ToLowerInvariant()} is now configured to be copied to container {command.ContainerUrl}.");
    }
}
