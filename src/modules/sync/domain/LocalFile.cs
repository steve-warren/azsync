namespace azsync;

/// <summary>
/// Represents a file on the local file system.
/// </summary>
/// <param name="Path">The path of the file.</param>
/// <param name="Name">The name of the file.</param>
/// <param name="LastModified">The time the file was last modified.</param>
/// <param name="PathHash">The hashed representation of the LocalFile's path.</param>
/// <param name="FileSizeInBytes">The size of the LocalFile in bytes.</param>
public record LocalFile(string Path, string Name, DateTime LastModified, string PathHash, long FileSizeInBytes);