namespace azsync;

public interface IFileSystem
{
    bool IsGlob(string path);
    bool IsFile(string path);
    bool IsDirectory(string path);
    LocalFile? GetFile(string path);
    IEnumerable<LocalFile> GetFiles(string path);
    IEnumerable<LocalFile> Glob(string glob);
}