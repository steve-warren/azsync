namespace azpush;

public class BlobFile
{
    private BlobFile() { }
    public BlobFile(string localFileName, string localFilePath, string localFilePathHash, int localPathId, string blobName)
    {
        LocalFileName = localFileName;
        LocalFilePath = localFilePath;
        LocalFilePathHash = localFilePathHash;
        LocalPathId = localPathId;
        BlobName = blobName;
        State = "Tracked";
    }

    public int Id { get; set; }
    public int LocalPathId { get; set; }
    public string LocalFileName { get; set; } = "";
    public string LocalFilePath { get; set; } = "";
    public string LocalFilePathHash { get; set; } = "";
    public string? BlobUrl { get; private set; }
    public string BlobName { get; private set; } = "";
    public DateTime LastModified { get; private set; }
    public DateTimeOffset? LastUpload { get; set; }
    public long FileSizeInBytes { get; private set; }
    public string ContentHash { get; private set; } = "";
    public string State { get; set; } = "";

    public void Upload(string blobUrl, string blobContentHash, DateTimeOffset timestamp)
    {
        if (string.Equals(blobContentHash, ContentHash, StringComparison.OrdinalIgnoreCase) is false)
            State = "Error";

        else
        {
            State = "Uploaded";
            LastUpload = timestamp;
            BlobUrl = blobUrl;
        }
    }

    public void Modify(DateTime lastModified, long fileSizeInBytes, string contentHash)
    {
        LastModified = lastModified;
        FileSizeInBytes = fileSizeInBytes;
        ContentHash = contentHash;
    }

    public void Error()
    {
        State = "Error";
    }

    public void Delete()
    {
        State = "Deleted";
    }
}