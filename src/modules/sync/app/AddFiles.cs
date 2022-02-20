using Microsoft.EntityFrameworkCore;

namespace azsync;

/// <summary>
/// Gathers file information for the specified directory.
/// </summary>
/// <param name="DirectoryPath">The path to the directory.</param>
/// <param name="MaxRecursionDepth"></param>
public record AddPath(string Path, string ContainerName) : ICommand { }

public class AddPathHandler : IAsyncCommandHandler<AddPath>
{
    private readonly IFileSystem _fs;
    private readonly SyncDbContext _context;

    public AddPathHandler(IFileSystem fileSystem, SyncDbContext context)
    {
        _fs = fileSystem;
        _context = context;
    }

    public async Task Handle(AddPath command)
    {
        var container = await _context.AzureContainers.FirstOrDefaultAsync(c => c.Name == command.ContainerName);
        
        if (container is null)
        {
            Console.WriteLine($"Cannot find container '{command.ContainerName}'");
            return;
        }

        var path = _fs.CreatePath(command.Path, container.Id);

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

        Console.WriteLine($"The {path.PathType.ToLowerInvariant()} is now configured to be copied to the {container.Name} container.");
    }
}
