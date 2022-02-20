namespace azsync;

public interface IFileSystem
{
    LocalPath CreatePath(string path, int containerId);
    LocalFile? GetFile(string path);
    IEnumerable<LocalFile> GetFiles(string path);
    IEnumerable<LocalFile> Glob(string glob);
}