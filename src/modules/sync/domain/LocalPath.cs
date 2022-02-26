namespace azpush;

public class LocalPath
{
    public LocalPath(string path, string pathType, int containerId)
    {
        Path = path;
        PathType = pathType;
        ContainerId = containerId;
    }

    public int Id { get; private set; }
    public string Path { get; private set; } = "";
    public string PathType { get; private set; }
    public int ContainerId { get; private set; }

    public IEnumerable<LocalFile> GetLocalFiles(IFileSystem fileSystem)
    {
        if (PathType == LocalPathType.Glob.Name)
            return fileSystem.Glob(this);

        else if (PathType == LocalPathType.Directory.Name)
            return fileSystem.GetFiles(this);

        else if (PathType == LocalPathType.File.Name)
        {
            var file = fileSystem.GetFile(this);
            
            if (file is not null) return new[] { file };
        }

        return Enumerable.Empty<LocalFile>();
    }
}