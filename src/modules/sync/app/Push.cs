namespace azsync;

/// <summary>
/// Gathers file information for the specified directory.
/// </summary>
public record Push() : ICommand { }

public class PushHandler : IAsyncCommandHandler<Push>
{
    private readonly ILocalFileRepository _localFiles;
    private readonly ISyncFileRepository _syncFiles;
    private IFileSystem _fs;
    private readonly SyncDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public PushHandler(SyncDbContext context, IFileSystem fileSystem, ILocalFileRepository localFileRepository, ISyncFileRepository syncFileRepository, IUnitOfWork unitOfWork)
    {
        _context = context;
        _fs = fileSystem;
        _localFiles = localFileRepository;
        _syncFiles = syncFileRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(Push _)
    {
        var localPaths = _context.LocalPaths.AsAsyncEnumerable();
        var localFiles = new List<LocalFile>();

        await foreach(var path in localPaths)
            localFiles.AddRange(path.GetLocalFiles(_fs));

        _localFiles.ReplaceAll(localFiles);

        var untrackedFiles = _localFiles.GetUntrackedFiles();

        foreach(var untrackedFile in untrackedFiles)
        {
            Console.WriteLine("untracked: " + untrackedFile.Name);
            var syncFile = new SyncFile(name: untrackedFile.Name, localFilePath: untrackedFile.Path, localFilePathHash: untrackedFile.PathHash, lastModified: untrackedFile.LastModified, fileSizeInBytes: untrackedFile.FileSizeInBytes, containerId: untrackedFile.ContainerId, localPathId: untrackedFile.LocalPathId);

            _syncFiles.Add(syncFile);
        }

        await _unitOfWork.SaveChangesAsync();
    }
}
