using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace azpush;

public class SyncDbContext : DbContext, IUnitOfWork
{
    public SyncDbContext()
    {
        Database.EnsureCreated();
    }

    public DbSet<LocalFileInfo> LocalFiles => Set<LocalFileInfo>();
    public DbSet<BlobFile> BlobFiles => Set<BlobFile>();
    public DbSet<LocalPath> LocalPaths => Set<LocalPath>();
    public DbSet<AzureCredential> AzureCredentials => Set<AzureCredential>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source=azpush.cache");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AzureCredential>()
                    .ToTable("AzureCredential")
                    .HasKey("Id");

        modelBuilder.Entity<LocalFileInfo>()
                    .ToTable("LocalFileInfo")
                    .HasKey("Path");
        
        modelBuilder.Entity<LocalPath>()
                    .ToTable("LocalPath")
                    .HasKey("Id");
        
        modelBuilder.Entity<LocalPath>()
                    .HasDiscriminator<string>("PathType")
                    .HasValue<GlobPath>(LocalPathType.Glob.Name)
                    .HasValue<FilePath>(LocalPathType.File.Name)
                    .HasValue<DirectoryPath>(LocalPathType.Directory.Name);
        
        modelBuilder.Entity<BlobFile>()
                    .Property(e => e.LocalFilePathHash).HasColumnName("LocalFilePathHash");

        modelBuilder.Entity<BlobFile>()
                    .ToTable("BlobFile")
                    .HasKey("Id");
    }

    Task IUnitOfWork.SaveChangesAsync()
    {
        return base.SaveChangesAsync();
    }
}