using Microsoft.Data.Sqlite;

namespace azsync;

public record CaptureLocalFiles(string Path, int MaxRecursionDepth) : ICommand { }

public class CaptureLocalFilesHandler : ICommandHandler<CaptureLocalFiles>
{
    private readonly IFileSystem _fs;
    private readonly ILocalFileRepository _localFileRepository;

    public CaptureLocalFilesHandler(IFileSystem fileSystem, ILocalFileRepository localFileRepository)
    {
        _fs = fileSystem;
        _localFileRepository = localFileRepository;
    }

    public void Handle(CaptureLocalFiles command)
    {
        var files = _fs.GetFilesInDirectory(directoryPath: command.Path, maxRecursionDepth: command.MaxRecursionDepth);

        _localFileRepository.ReplaceAll(files);
    }
}
