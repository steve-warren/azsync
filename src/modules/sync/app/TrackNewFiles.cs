namespace azsync;

/// <summary>
/// Gathers file information for the specified directory.
/// </summary>
public record TrackNewFiles() : ICommand { }

public class TrackNewFilesHandler : ICommandHandler<TrackNewFiles>
{
    private readonly ILocalFileRepository _localFileRepository;
    private readonly ISyncFileRepository _syncFileRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TrackNewFilesHandler(ILocalFileRepository localFileRepository, ISyncFileRepository syncFileRepository, IUnitOfWork unitOfWork)
    {
        _localFileRepository = localFileRepository;
        _syncFileRepository = syncFileRepository;
        _unitOfWork = unitOfWork;
    }

    public void Handle(TrackNewFiles _)
    {
        var untrackedFiles = _localFileRepository.GetUntrackedFiles();

        foreach(var untrackedFile in untrackedFiles)
        {
            var syncFile = new SyncFile(name: untrackedFile.Name, localFilePath: untrackedFile.Path, localFilePathHash: untrackedFile.PathHash, lastModified: untrackedFile.LastModified, fileSizeInBytes: untrackedFile.FileSizeInBytes);

            _syncFileRepository.Add(syncFile);
        }

        _unitOfWork.SaveChangesAsync();
    }
}
