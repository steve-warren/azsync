namespace azsync;

/// <summary>
/// Gathers file information for the specified directory.
/// </summary>
/// <param name="DirectoryPath">The path to the directory.</param>
/// <param name="MaxRecursionDepth"></param>
public record AddPath(string Path) : ICommand { }

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
        var path = _fs.GetPath(command.Path);

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

        Console.WriteLine("Path added.");
    }
}
