namespace azsync;

/// <summary>
/// Gathers file information for the specified directory.
/// </summary>
/// <param name="DirectoryPath">The path to the directory.</param>
/// <param name="MaxRecursionDepth"></param>
public record CaptureLocalDirectory(string DirectoryPath, int MaxRecursionDepth) : ICommand { }

public class CaptureLocalDirectoryHandler : ICommandHandler<CaptureLocalDirectory>
{
    private readonly IFileSystem _fs;
    private readonly ILocalFileRepository _localFileRepository;

    public CaptureLocalDirectoryHandler(IFileSystem fileSystem, ILocalFileRepository localFileRepository)
    {
        _fs = fileSystem;
        _localFileRepository = localFileRepository;
    }

    public void Handle(CaptureLocalDirectory command)
    {
        var files = _fs.GetFilesInDirectory(directoryPath: command.DirectoryPath, maxRecursionDepth: command.MaxRecursionDepth);

        _localFileRepository.ReplaceAll(files);
    }
}
