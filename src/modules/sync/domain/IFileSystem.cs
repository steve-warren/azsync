namespace azpush;

public interface IFileSystem
{
    LocalPath CreatePath(string path, int containerId);
    LocalFile? GetFile(LocalPath path);
    IEnumerable<LocalFile> GetFiles(LocalPath path);
    IEnumerable<LocalFile> Glob(LocalPath path);
}