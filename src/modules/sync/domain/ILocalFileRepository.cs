namespace azsync;

public interface ILocalFileRepository
{
    void Add(LocalFile file);
    IQueryable<LocalFile> GetNewLocalFiles();
}
