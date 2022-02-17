namespace azsync;

public class AzureCredential
{

    private AzureCredential() { }
    public AzureCredential(string name, string tenant, string client, string secret)
    {
        this.Name = name;
        this.Tenant = tenant;
        this.Client = client;
        this.Secret = secret;
    }

    public int Id { get; private set; }
    public string Name { get; private set; } = "";
    public string Tenant { get; private set; } = "";
    public string Client { get; private set; } = "";
    public string Secret { get; private set; } = "";
}
