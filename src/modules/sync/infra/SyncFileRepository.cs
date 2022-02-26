namespace azpush;

public class SyncFileRepository : ISyncFileRepository
{
    private readonly SyncDbContext _context;

    public SyncFileRepository(SyncDbContext context)
    {
        _context = context;
    }

    public void Add(RemoteFile file) => _context.RemoteFiles.Add(file);
    public void Remove(RemoteFile file) => _context.RemoteFiles.Remove(file);

    public IQueryable<RemoteFile> GetDeleted(int pathId) =>
        from sf in _context.RemoteFiles
        join lf in _context.LocalFiles on sf.LocalFilePathHash equals lf.PathHash into group_join
        from default_lf in group_join.DefaultIfEmpty()
        where default_lf == null && sf.LocalPathId == pathId
        select sf;
    
}