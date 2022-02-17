using Microsoft.EntityFrameworkCore;

namespace azsync;

public class AzureCredentialRepository : IAzureCredentialRepository
{
    private readonly SyncDbContext _context;
    public AzureCredentialRepository(SyncDbContext context) { _context = context; }
    public void Add(AzureCredential credential) => _context.AzureCredentials.Add(credential);
    public Task<AzureCredential?> GetByNameAsync(string name) => _context.AzureCredentials.FirstOrDefaultAsync(c => c.Name == name);
    public void Remove(AzureCredential credential) => _context.AzureCredentials.Remove(credential);
    public Task<bool> ExistsAsync(string name) => _context.AzureCredentials.AnyAsync(c => c.Name == name);
    public IAsyncEnumerable<AzureCredential> ListAsync() => _context.AzureCredentials.AsAsyncEnumerable();
}