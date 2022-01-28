namespace azsync;

public interface ILocalFileRepository
{
    /// <summary>
    /// Replaces all existing LocalFile entities with the specified LocalFile entities.
    /// </summary>
    /// <param name="files"></param>
    void ReplaceAll(IEnumerable<LocalFile> files);
    /// <summary>
    /// Returns LocalFile entities that have not been synched.
    /// </summary>
    /// <returns></returns>
    IQueryable<LocalFile> GetNewLocalFiles();
}
