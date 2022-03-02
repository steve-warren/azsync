namespace azpush;

public class FileSystem : IFileSystem
{
    private readonly IHashAlgorithm _hash;

    public FileSystem(IHashAlgorithm hash)
    {
        _hash = hash;
    }

    public LocalPath CreatePath(string path, int containerId, string? blobName)
    {
        if (IsFile(path)) return new FilePath(path: path, containerId: containerId, blobName: blobName);
        if (IsDirectory(path)) return new DirectoryPath(path: path, containerId: containerId);
        if (IsGlob(path)) return new GlobPath(path: path, containerId: containerId);

        return new InvalidPath(path: path, containerId: containerId);
    }

    public LocalFileInfo? GetFile(LocalPath path)
    {
        var info = new System.IO.FileInfo(path.Path);

        if (info.Exists is false) return default;
        
        return new LocalFileInfo(Path: info.FullName, Name: info.Name, LastModified: info.LastWriteTime, PathHash: _hash.ComputeHash(info.FullName), FileSizeInBytes: info.Length, LocalPathId: path.Id, ContainerId: path.ContainerId);
    }

    public IEnumerable<LocalFileInfo> GetFiles(LocalPath path)
    {
        foreach(var file in Directory.EnumerateFiles(path: path.Path))
        {
            var info = new System.IO.FileInfo(file);

            yield return new LocalFileInfo(Path: info.FullName, Name: info.Name, LastModified: info.LastWriteTime, PathHash: _hash.ComputeHash(info.FullName), FileSizeInBytes: info.Length, LocalPathId: path.Id, ContainerId: path.ContainerId);
        }
    }

    public IEnumerable<LocalFileInfo> Glob(LocalPath path)
    {
        var globInfo = new FileInfo(path.Path);
        var fullPath = globInfo.DirectoryName;
        var pattern = globInfo.Name;

        var files = Directory.EnumerateFiles(path: fullPath!, searchPattern: pattern);
        
        foreach(var file in files)
        {
            var info = new System.IO.FileInfo(file);

            yield return new LocalFileInfo(Path: info.FullName, Name: info.Name, LastModified: info.LastWriteTime, PathHash: _hash.ComputeHash(info.FullName), FileSizeInBytes: info.Length, LocalPathId: path.Id, ContainerId: path.ContainerId);
        }
    }

    public Stream OpenFile(string path)
    {
        return new FileStream(path: path, mode: FileMode.Open, access: FileAccess.Read, share: FileShare.Read, bufferSize: 4096, useAsync: true);
    }

    private static bool IsGlob(string path) => path.LastIndexOfAny(new[] { '*', '?' }) != -1;
    private static bool IsFile(string path) => new FileInfo(path).Exists;
    private static bool IsDirectory(string path) => new DirectoryInfo(path).Exists;
}
