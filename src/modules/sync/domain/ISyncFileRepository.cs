namespace azsync;

public interface ISyncFileRepository
{
    void Add(SyncFile file);
    IQueryable<SyncFile> GetDeleted(int pathId);
    void Remove(SyncFile file);
}