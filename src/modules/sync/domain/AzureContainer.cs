namespace azsync;

public class AzureContainer
{
    public AzureContainer(string name, string containerUrl, int credentialId)
    {
        Name = name;
        ContainerUrl = containerUrl;
        CredentialId = credentialId;
    }

    public int Id { get; private set; }
    public string Name { get; private set; }
    public string ContainerUrl { get; init; }
    public int CredentialId { get; init; }
}