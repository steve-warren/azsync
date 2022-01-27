namespace azsync;

public class LocalFileRepository : ILocalFileRepository
{
    private readonly SyncDbContext _context;

    public LocalFileRepository(SyncDbContext context)
    {
        _context = context;
    }
    public void Add(LocalFile file) => _context.Add(file);

    public IQueryable<LocalFile> GetNewLocalFiles()
    {
        var query = from lf in _context.LocalFiles
                    join sf in _context.SyncFiles on lf.PathHash equals sf.LocalFilePathHash into group_join
                    from default_sf in group_join.DefaultIfEmpty()
                    select lf;
        
        return query;
    }
}
