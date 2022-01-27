namespace azsync;

public class SyncFileRepository : ISyncFileRepository
{
    private readonly SyncDbContext _context;

    public SyncFileRepository(SyncDbContext context)
    {
        _context = context;
    }

    public void Add(SyncFile file)
    {
        _context.SyncFiles.Add(file);
    }
}