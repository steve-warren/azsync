namespace azsync;

public class LocalPath
{
    public LocalPath(string path, string pathType)
    {
        Path = path;
        PathType = pathType;
    }

    public int Id { get; private set; }
    public string Path { get; private set; } = "";
    public string PathType { get; private set; }
}