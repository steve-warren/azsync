namespace azpush;

public class FileSystem : IFileSystem
{
    private readonly IHashAlgorithm _hash;

    public FileSystem(IHashAlgorithm hash)
    {
        _hash = hash;
    }

    public LocalPath CreatePath(string path, int credentialId, string containerUrl, string? blobName, bool includeTimestamp)
    {
        if (IsFile(path)) return new FilePath(path: path, credentialId: credentialId, containerUrl: containerUrl, blobName: blobName, includeTimestamp: includeTimestamp);
        if (IsDirectory(path)) return new DirectoryPath(path: path, credentialId: credentialId, containerUrl: containerUrl, includeTimestamp: includeTimestamp);
        if (IsGlob(path)) return new GlobPath(path: path, credentialId: credentialId, containerUrl: containerUrl, includeTimestamp: includeTimestamp);

        return new InvalidPath(path: path, credentialId: credentialId, containerUrl: containerUrl);
    }

    public LocalFileInfo? GetFile(LocalPath path)
    {
        var info = new FileInfo(path.Path);

        if (info.Exists is false) return default;
        
        return new LocalFileInfo(Path: info.FullName, Name: info.Name, LastModified: info.LastWriteTime, PathHash: _hash.ComputeHash(info.FullName), FileSizeInBytes: info.Length, LocalPathId: path.Id, ContainerUrl: path.ContainerUrl);
    }

    public IEnumerable<LocalFileInfo> GetFiles(LocalPath path)
    {
        foreach(var file in Directory.EnumerateFiles(path: path.Path))
        {
            var info = new FileInfo(file);

            yield return new LocalFileInfo(Path: info.FullName, Name: info.Name, LastModified: info.LastWriteTime, PathHash: _hash.ComputeHash(info.FullName), FileSizeInBytes: info.Length, LocalPathId: path.Id, ContainerUrl: path.ContainerUrl);
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
            var info = new FileInfo(file);

            yield return new LocalFileInfo(Path: info.FullName, Name: info.Name, LastModified: info.LastWriteTime, PathHash: _hash.ComputeHash(info.FullName), FileSizeInBytes: info.Length, LocalPathId: path.Id, ContainerUrl: path.ContainerUrl);
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
