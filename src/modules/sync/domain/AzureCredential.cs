namespace azpush;

public class AzureCredential
{

    private AzureCredential() { }
    public AzureCredential(string name, string tenant, string client)
    {
        this.Name = name;
        this.Tenant = tenant;
        this.Client = client;
    }

    public int Id { get; private set; }
    public string Name { get; private set; } = "";
    public string Tenant { get; set; } = "";
    public string Client { get; set; } = "";
    public string Secret { get; private set; } = "";

    public void SetSecret(string secret, IStringProtector protector)
    {
        Secret = protector.ProtectString(secret);
    }

    public string GetSecret(IStringProtector protector)
    {
        return protector.UnprotectString(Secret);
    }
}
