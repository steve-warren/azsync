namespace azsync;

public class FileSystem : IFileSystem
{
    private readonly IHashAlgorithm _hash;

    public FileSystem(IHashAlgorithm hash)
    {
        _hash = hash;
    }

    public IEnumerable<LocalFile> GetFilesInDirectory(string directoryPath, int maxRecursionDepth)
    {
        var query = Directory.EnumerateFiles(path: directoryPath, searchPattern: "*", new EnumerationOptions { RecurseSubdirectories = true, MaxRecursionDepth = maxRecursionDepth });

        foreach(var file in query)
        {
            var info = new FileInfo(file);

            yield return new LocalFile(Path: info.FullName, Name: info.Name, LastModified: info.LastWriteTime, PathHash: _hash.ComputeHash(info.FullName), FileSizeInBytes: info.Length);
        }
    }
}
