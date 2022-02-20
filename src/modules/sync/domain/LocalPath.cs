namespace azsync;

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
}