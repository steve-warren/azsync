namespace azpush;

public interface IBlobFileRepository
{
    void Add(BlobFile file);
    IQueryable<BlobFile> GetDeleted(int pathId);
    void Remove(BlobFile file);
}