namespace azsync;

/// <summary>
/// Gathers file information for the specified directory.
/// </summary>
public record TrackLocalPathChanges() : ICommand { }

public class TrackLocalPathChangesHandler : IAsyncCommandHandler<TrackLocalPathChanges>
{
    private readonly ILocalFileRepository _localFileRepository;
    private readonly ISyncFileRepository _syncFileRepository;
    private IFileSystem _fs;
    private readonly SyncDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public TrackLocalPathChangesHandler(SyncDbContext context, IFileSystem fileSystem, ILocalFileRepository localFileRepository, ISyncFileRepository syncFileRepository, IUnitOfWork unitOfWork)
    {
        _context = context;
        _fs = fileSystem;
        _localFileRepository = localFileRepository;
        _syncFileRepository = syncFileRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(TrackLocalPathChanges _)
    {
        var paths = _context.LocalPaths.AsAsyncEnumerable();

        await foreach(var path in paths)
        {
            if (path.PathType == "Glob")
            {
                foreach(var file in _fs.Glob(path.Path))
                {
                    Console.WriteLine(file.Path);
                }
            }

            else if (path.PathType == "Directory")
            {
                foreach(var file in _fs.GetFiles(path.Path))
                {
                    Console.WriteLine(file.Path);
                }
            }

            else if (path.PathType == "File")
            {
                var file = _fs.GetFile(path.Path);
                Console.WriteLine(file?.Path);
            }
        }
    }
}
