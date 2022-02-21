namespace azsync;

public interface ISyncFileRepository
{
    void Add(SyncFile file);
    IQueryable<SyncFile> GetDeleted();
    void Remove(SyncFile file);
}