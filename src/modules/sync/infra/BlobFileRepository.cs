namespace azpush;

public class BlobFileRepository : IBlobFileRepository
{
    private readonly SyncDbContext _context;

    public BlobFileRepository(SyncDbContext context)
    {
        _context = context;
    }

    public void Add(BlobFile file) => _context.BlobFiles.Add(file);
    public void Remove(BlobFile file) => _context.BlobFiles.Remove(file);

    public IQueryable<BlobFile> GetDeleted(int pathId) =>
        from sf in _context.BlobFiles
        join lf in _context.LocalFiles on sf.LocalFilePathHash equals lf.PathHash into group_join
        from default_lf in group_join.DefaultIfEmpty()
        where default_lf == null && sf.LocalPathId == pathId
        select sf;
    
}