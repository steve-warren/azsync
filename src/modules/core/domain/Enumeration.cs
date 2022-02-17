namespace azsync;

public abstract record Enumeration
{
    public string Name { get; init; } = "";

    public override string ToString() => Name;
}