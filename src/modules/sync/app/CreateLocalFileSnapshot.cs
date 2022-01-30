namespace azsync;

/// <summary>
/// Gathers file information for the specified directory.
/// </summary>
/// <param name="DirectoryPath">The path to the directory.</param>
/// <param name="MaxRecursionDepth"></param>
public record CreateLocalFileSnapshot(string DirectoryPath, int MaxRecursionDepth) : ICommand { }

public class CreateLocalFileSnapshotHandler : ICommandHandler<CreateLocalFileSnapshot>
{
    private readonly IFileSystem _fs;
    private readonly ILocalFileRepository _localFileRepository;

    public CreateLocalFileSnapshotHandler(IFileSystem fileSystem, ILocalFileRepository localFileRepository)
    {
        _fs = fileSystem;
        _localFileRepository = localFileRepository;
    }

    public void Handle(CreateLocalFileSnapshot command)
    {
        var files = _fs.GetFilesInDirectory(directoryPath: command.DirectoryPath, maxRecursionDepth: command.MaxRecursionDepth);

        _localFileRepository.ReplaceAll(files);
    }
}
