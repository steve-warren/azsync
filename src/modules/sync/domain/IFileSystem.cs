namespace azsync;

public interface IFileSystem
{
    IEnumerable<LocalFile> GetFilesInDirectory(string directoryPath, int maxRecursionDepth);
}