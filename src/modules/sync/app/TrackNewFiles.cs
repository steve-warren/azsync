namespace azsync;

/// <summary>
/// Gathers file information for the specified directory.
/// </summary>
public record TrackLocalPathChanges() : ICommand { }

public class TrackLocalPathChangesHandler : IAsyncCommandHandler<TrackLocalPathChanges>
{
    private readonly ILocalFileRepository _localFiles;
    private readonly ISyncFileRepository _syncFiles;
    private IFileSystem _fs;
    private readonly SyncDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public TrackLocalPathChangesHandler(SyncDbContext context, IFileSystem fileSystem, ILocalFileRepository localFileRepository, ISyncFileRepository syncFileRepository, IUnitOfWork unitOfWork)
    {
        _context = context;
        _fs = fileSystem;
        _localFiles = localFileRepository;
        _syncFiles = syncFileRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(TrackLocalPathChanges _)
    {
        var localPaths = _context.LocalPaths.AsAsyncEnumerable();
        var localFiles = new List<LocalFile>();

        await foreach(var path in localPaths)
        {
            if (path.PathType == LocalPathType.Glob.Name)
                localFiles.AddRange(_fs.Glob(path.Path));

            else if (path.PathType == LocalPathType.Directory.Name)
                localFiles.AddRange(_fs.GetFiles(path.Path));

            else if (path.PathType == LocalPathType.File.Name)
            {
                var file = _fs.GetFile(path.Path);
                
                if (file is null) continue;
                
                localFiles.Add(file);
            }
        }

        _localFiles.ReplaceAll(localFiles);

        foreach(var file in localFiles)
            Console.WriteLine(file.Path);
    }
}
