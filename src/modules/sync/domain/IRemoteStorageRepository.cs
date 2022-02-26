namespace azpush;

public interface IAzureContainerRepository
{
    void Add(AzureContainer container);
    void Remove(AzureContainer container);
    Task<AzureContainer?> GetByNameAsync(string name);
    Task<bool> ExistsAsync(string name);
    IAsyncEnumerable<AzureContainer> ListAsync();
}