using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace azpush;

public class SyncDbContext : DbContext, IUnitOfWork
{
    public SyncDbContext()
    {
        Database.EnsureCreated();
    }

    public DbSet<LocalFile> LocalFiles => Set<LocalFile>();
    public DbSet<SyncFile> SyncFiles => Set<SyncFile>();
    public DbSet<LocalPath> LocalPaths => Set<LocalPath>();
    public DbSet<AzureCredential> AzureCredentials => Set<AzureCredential>();
    public DbSet<AzureContainer> AzureContainers => Set<AzureContainer>();
    public DbSet<ConfigurationSetting> ConfigurationSettings => Set<ConfigurationSetting>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source=azpush.cache");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AzureCredential>()
                    .ToTable("AzureCredential")
                    .HasKey("Id");

        modelBuilder.Entity<AzureContainer>()
                    .ToTable("AzureContainer")
                    .HasKey("Id");

        modelBuilder.Entity<LocalFile>()
                    .ToTable("LocalFile")
                    .HasKey("Path");
        
        modelBuilder.Entity<LocalPath>()
                    .ToTable("LocalPath")
                    .HasKey("Id");
        
        modelBuilder.Entity<SyncFile>()
                    .Property(e => e.LocalFilePathHash).HasColumnName("LocalFilePathHash");

        modelBuilder.Entity<SyncFile>()
                    .ToTable("SyncFile")
                    .HasKey("Id");

        modelBuilder.Entity<ConfigurationSetting>()
                    .ToTable("ConfigurationSetting")
                    .HasKey("Key");
    }

    Task IUnitOfWork.SaveChangesAsync()
    {
        return base.SaveChangesAsync();
    }
}