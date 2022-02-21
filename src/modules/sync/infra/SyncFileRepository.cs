namespace azsync;

public class SyncFileRepository : ISyncFileRepository
{
    private readonly SyncDbContext _context;

    public SyncFileRepository(SyncDbContext context)
    {
        _context = context;
    }

    public void Add(SyncFile file) => _context.SyncFiles.Add(file);
    public void Remove(SyncFile file) => _context.SyncFiles.Remove(file);

    public IQueryable<SyncFile> GetDeleted() =>
        from sf in _context.SyncFiles
        join lf in _context.LocalFiles on sf.LocalFilePathHash equals lf.PathHash into group_join
        from default_lf in group_join.DefaultIfEmpty()
        where default_lf == null
        select sf;
    
}