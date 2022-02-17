namespace azsync;

public class FileSystem : IFileSystem
{
    private readonly IHashAlgorithm _hash;

    public FileSystem(IHashAlgorithm hash)
    {
        _hash = hash;
    }

    public IEnumerable<LocalFile> GetFilesInDirectory(DirectoryQuery query)
    {
        var files = Directory.EnumerateFiles(path: query.Path, searchPattern: query.SearchPattern, new EnumerationOptions { RecurseSubdirectories = query.Recursive, MaxRecursionDepth = query.MaxRecursionDepth });

        foreach(var file in files)
        {
            var info = new FileInfo(file);

            yield return new LocalFile(Path: info.FullName, Name: info.Name, LastModified: info.LastWriteTime, PathHash: _hash.ComputeHash(info.FullName), FileSizeInBytes: info.Length);
        }
    }
}
