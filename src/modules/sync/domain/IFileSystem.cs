namespace azpush;

public interface IFileSystem
{
    LocalPath CreatePath(string path, int containerId, string? blobName);
    LocalFileInfo? GetFile(LocalPath path);
    IEnumerable<LocalFileInfo> GetFiles(LocalPath path);
    IEnumerable<LocalFileInfo> Glob(LocalPath path);
    Stream OpenFile(string path);
}