namespace azsync;

public interface ILocalFileRepository
{
    /// <summary>
    /// Replaces all existing LocalFile entities with the specified LocalFile entities in the repository.
    /// </summary>
    /// <param name="files"></param>
    void ReplaceAll(IEnumerable<LocalFile> files);
    /// <summary>
    /// Returns LocalFile entities that have not been synched.
    /// </summary>
    /// <returns></returns>
    IQueryable<LocalFile> GetUntrackedFiles();
}
