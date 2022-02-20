namespace azsync;

public interface IFileSystem
{
    LocalPath GetPath(string path);
    LocalFile? GetFile(string path);
    IEnumerable<LocalFile> GetFiles(string path);
    IEnumerable<LocalFile> Glob(string glob);
}