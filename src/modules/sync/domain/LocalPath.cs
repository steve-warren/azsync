namespace azpush;

public class GlobPath : LocalPath
{
    public GlobPath(string path, int credentialId, string containerUrl, bool includeTimestamp)
    : base(path, LocalPathType.Glob.Name, credentialId, containerUrl, null, includeTimestamp) { }
    public override IEnumerable<LocalFileInfo> GetFiles(IFileSystem fileSystem) => fileSystem.Glob(this);
}

public class FilePath : LocalPath
{
    public FilePath(string path, int credentialId, string containerUrl, string? blobName, bool includeTimestamp)
    : base(path, LocalPathType.File.Name, credentialId, containerUrl, blobName: blobName, includeTimestamp) { }
    public override IEnumerable<LocalFileInfo> GetFiles(IFileSystem fileSystem)
    {
        var file = fileSystem.GetFile(this);
        if (file is not null) return new[] { file };
        return Enumerable.Empty<LocalFileInfo>();
    }
}

public class DirectoryPath : LocalPath
{
    public DirectoryPath(string path, int credentialId, string containerUrl, bool includeTimestamp)
    : base(path, LocalPathType.Directory.Name, credentialId, containerUrl, null, includeTimestamp) { }
    public override IEnumerable<LocalFileInfo> GetFiles(IFileSystem fileSystem) => fileSystem.GetFiles(this);
}

public class InvalidPath : LocalPath
{
    public InvalidPath(string path, int credentialId, string containerUrl)
    : base(path, LocalPathType.Invalid.Name, credentialId, containerUrl, includeTimestamp: false) { }
    public override IEnumerable<LocalFileInfo> GetFiles(IFileSystem fileSystem) => throw new InvalidOperationException("Invalid path.");
}

public class LocalPath
{
    protected LocalPath(string path, string pathType, int credentialId, string containerUrl, string? blobName = default, bool includeTimestamp = false)
    {
        Path = path;
        PathType = pathType;
        CredentialId = credentialId;
        ContainerUrl = containerUrl;
        BlobName = blobName;
        IncludeTimestamp = includeTimestamp;
    }

    public int Id { get; private set; }
    public string Path { get; private set; } = "";
    public string PathType { get; private set; }
    public int CredentialId { get; private set; }
    public string ContainerUrl { get; private set; }
    public string? BlobName { get; protected set; }
    public bool IncludeTimestamp { get; protected set; }

    public virtual IEnumerable<LocalFileInfo> GetFiles(IFileSystem fileSystem)
    {
        throw new InvalidOperationException();
    }
}