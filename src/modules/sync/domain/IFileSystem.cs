namespace azsync;

public interface IFileSystem
{
    IEnumerable<LocalFile> GetFilesInDirectory(DirectoryQuery query);
}

public record DirectoryQuery(string Path, string SearchPattern = "*", bool Recursive = false, int MaxRecursionDepth = int.MaxValue);