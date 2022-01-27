using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace azsync;

public class SyncUnitOfWork : IUnitOfWork
{
    private readonly SyncDbContext _context;

    public SyncUnitOfWork(SyncDbContext context)
    {
        _context = context;
    }

    public void SaveChanges() => _context.SaveChanges();
}