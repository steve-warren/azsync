using Microsoft.EntityFrameworkCore;

namespace azsync;

public class AzureContainerRepository : IAzureContainerRepository
{
    private readonly SyncDbContext _context;
    public AzureContainerRepository(SyncDbContext context) { _context = context; }
    public void Add(AzureContainer container) => _context.AzureContainers.Add(container);
    public Task<bool> ExistsAsync(string name) => _context.AzureContainers.AnyAsync(c => c.Name == name);
    public Task<AzureContainer?> GetByNameAsync(string name) => _context.AzureContainers.FirstOrDefaultAsync(c => c.Name == name);
    public IAsyncEnumerable<AzureContainer> ListAsync() => _context.AzureContainers.AsAsyncEnumerable();
    public void Remove(AzureContainer container) => _context.AzureContainers.Remove(container);
}