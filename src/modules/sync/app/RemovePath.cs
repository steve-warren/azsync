using Microsoft.EntityFrameworkCore;

namespace azpush;

public record RemovePath(string Path) : ICommand { }

public class RemovePathHandler : IAsyncCommandHandler<RemovePath>
{
    private readonly SyncDbContext _context;

    public RemovePathHandler(SyncDbContext context)
    {
        _context = context;
    }
    
    public async Task Handle(RemovePath command)
    {
        var path = await _context.LocalPaths.FirstOrDefaultAsync(p => p.Path == command.Path);
        
        if (path is null)
        {
            Console.WriteLine("Path not found.");
            return;
        }

        var files = await _context.BlobFiles.Where(sf => sf.LocalPathId == path.Id).ToListAsync();

        _context.LocalPaths.Remove(path);
        _context.BlobFiles.RemoveRange(files);

        await _context.SaveChangesAsync();
        Console.WriteLine("Path removed.");
    }
}