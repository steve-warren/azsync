namespace azsync;

/// <summary>
/// Gathers file information for the specified directory.
/// </summary>
/// <param name="DirectoryPath">The path to the directory.</param>
/// <param name="MaxRecursionDepth"></param>
public record AddFiles(string Path, string SearchPattern, bool Recursive = false, int MaxRecursionDepth = int.MaxValue) : ICommand { }

public class AddFilesHandler : ICommandHandler<AddFiles>
{
    private readonly IFileSystem _fs;
    private readonly ILocalFileRepository _localFileRepository;

    public AddFilesHandler(IFileSystem fileSystem, ILocalFileRepository localFileRepository)
    {
        _fs = fileSystem;
        _localFileRepository = localFileRepository;
    }

    public void Handle(AddFiles command)
    {
        // todo refactor
        var query = new DirectoryQuery(Path: command.Path, SearchPattern: command.SearchPattern);

        var type = FilePathType.Invalid;

        var isGlob = command.Path.LastIndexOfAny(new[] { '*', '?' }) != -1;
        var isFile = new FileInfo(command.Path).Exists;
        var isDirectory = new DirectoryInfo(command.Path).Exists;
        var isInvalid = !(isFile ^ isDirectory ^ isGlob);

        if (isInvalid) type = FilePathType.Invalid;
        else if (isFile) type = FilePathType.File;
        else if (isDirectory) type = FilePathType.Directory;
        else if (isGlob) type = FilePathType.Glob;

        var s = type switch
        {
            FilePathType.Invalid => "invalid",
            FilePathType.File => "file",
            FilePathType.Directory => "dir",
            FilePathType.Glob => "glob",
            _ => throw new NotImplementedException()
        };

        Console.WriteLine(s);
        return;

        var files = _fs.GetFilesInDirectory(query);

        foreach(var file in files)
            Console.WriteLine(file.Name + " " + file.LastModified);

        //_localFileRepository.ReplaceAll(files);
    }
}

public enum FilePathType
{
    Invalid,
    File,
    Directory,
    Glob
}