namespace azsync;

public class FileSystem : IFileSystem
{
    private readonly IHashAlgorithm _hash;

    public FileSystem(IHashAlgorithm hash)
    {
        _hash = hash;
    }

    public LocalPath GetPath(string path)
    {
        var type = LocalPathType.Invalid;

        var isGlob = IsGlob(path);
        var isFile = IsFile(path);
        var isDirectory = IsDirectory(path);
        var isInvalid = !(isFile ^ isDirectory ^ isGlob);
        
        if (isInvalid) type = LocalPathType.Invalid;
        else if (isFile) type = LocalPathType.File;
        else if (isDirectory) type = LocalPathType.Directory;
        else if (isGlob) type = LocalPathType.Glob;

        return new LocalPath(path, type.Name);
    }

    public LocalFile? GetFile(string path)
    {
        var info = new FileInfo(path);

        if (info.Exists is false) return default;
        
        return new LocalFile(Path: info.FullName, Name: info.Name, LastModified: info.LastWriteTime, PathHash: _hash.ComputeHash(info.FullName), FileSizeInBytes: info.Length);
    }

    public IEnumerable<LocalFile> GetFiles(string path)
    {
        foreach(var file in Directory.EnumerateFiles(path: path))
        {
            var info = new FileInfo(file);

            yield return new LocalFile(Path: info.FullName, Name: info.Name, LastModified: info.LastWriteTime, PathHash: _hash.ComputeHash(info.FullName), FileSizeInBytes: info.Length);
        }
    }

    public IEnumerable<LocalFile> Glob(string glob)
    {
        var globInfo = new FileInfo(glob);
        var fullPath = globInfo.DirectoryName;
        var pattern = globInfo.Name;

        var files = Directory.EnumerateFiles(path: fullPath!, searchPattern: pattern);

        foreach(var file in files)
        {
            var info = new FileInfo(file);

            yield return new LocalFile(Path: info.FullName, Name: info.Name, LastModified: info.LastWriteTime, PathHash: _hash.ComputeHash(info.FullName), FileSizeInBytes: info.Length);
        }
    }

    private static bool IsGlob(string path) => path.LastIndexOfAny(new[] { '*', '?' }) != -1;
    private static bool IsFile(string path) => new FileInfo(path).Exists;
    private static bool IsDirectory(string path) => new DirectoryInfo(path).Exists;
}
