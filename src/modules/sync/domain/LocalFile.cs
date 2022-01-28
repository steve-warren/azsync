namespace azsync;

public record LocalFile(string Path, string Name, DateTime LastModified, string PathHash, long FileSizeInBytes);