namespace azsync;

public record LocalPathType : Enumeration
{
    public static readonly LocalPathType Invalid = new() { Name = nameof(Invalid) };
    public static readonly LocalPathType File = new() { Name = nameof(File) };
    public static readonly LocalPathType Directory = new() { Name = nameof(Directory) };
    public static readonly LocalPathType Glob = new() { Name = nameof(Glob) };
}