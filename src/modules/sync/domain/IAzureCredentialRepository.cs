namespace azpush;

public interface IAzureCredentialRepository
{
    void Add(AzureCredential credential);
    void Remove(AzureCredential credential);
    Task<AzureCredential?> GetByNameAsync(string name);
    Task<bool> ExistsAsync(string name);
    IAsyncEnumerable<AzureCredential> ListAsync();
}