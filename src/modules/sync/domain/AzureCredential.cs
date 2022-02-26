namespace azpush;

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
    public string Name { get; set; } = "";
    public string Tenant { get; set; } = "";
    public string Client { get; set; } = "";
    public string Secret { get; set; } = "";
}
