namespace azpush;

public interface ISyncFileRepository
{
    void Add(RemoteFile file);
    IQueryable<RemoteFile> GetDeleted(int pathId);
    void Remove(RemoteFile file);
}