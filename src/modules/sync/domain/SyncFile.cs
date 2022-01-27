namespace azsync;

public class SyncFile
{
    private SyncFile() { }
    public SyncFile(int id, string name, string localFilePath, string localFilePathHash, DateTime lastModified)
    {
        Id = id;
        Name = name;
        LocalFilePath = localFilePath;
        LocalFilePathHash = localFilePathHash;
        LastModified = lastModified;
        State = "NotSynced";
    }

    public int Id { get; }
    public string Name { get; }
    public string LocalFilePath { get; }
    public string LocalFilePathHash { get; }
    public string RemoteFilePath { get; }
    public DateTime LastModified { get; }
    public string State { get; }
}