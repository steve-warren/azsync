namespace azsync;

/// <summary>
/// Gathers file information for the specified directory.
/// </summary>
/// <param name="DirectoryPath">The path to the directory.</param>
/// <param name="MaxRecursionDepth"></param>
public record AddFiles(string Path) : ICommand { }

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
        var type = FilePathType.Invalid;

        var isGlob = _fs.IsGlob(command.Path);
        var isFile = _fs.IsFile(command.Path);
        var isDirectory = _fs.IsDirectory(command.Path);
        var isInvalid = !(isFile ^ isDirectory ^ isGlob);

        if (isInvalid) type = FilePathType.Invalid;
        else if (isFile) type = FilePathType.File;
        else if (isDirectory) type = FilePathType.Directory;
        else if (isGlob) type = FilePathType.Glob;

        var files = new List<LocalFile>();

        if (type is FilePathType.Invalid)
        {
            Console.WriteLine("Invalid glob, file or directory path.");
            return;
        }

        else if (type is FilePathType.Glob)
            files.AddRange(_fs.Glob(glob: command.Path));

        else if (type is FilePathType.Directory)
            files.AddRange(_fs.GetFiles(path: command.Path));

        else if (type is FilePathType.File)
        {
            var file = _fs.GetFile(path: command.Path);
            if (file is not null) files.Add(file);
        }

        _localFileRepository.ReplaceAll(files);
    }
}

public enum FilePathType
{
    Invalid,
    File,
    Directory,
    Glob
}