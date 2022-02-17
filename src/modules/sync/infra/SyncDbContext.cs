using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace azsync;

public class SyncDbContext : DbContext, IUnitOfWork
{
    public SyncDbContext()
    {
        Database.EnsureCreated();
    }

    public DbSet<LocalFile> LocalFiles { get; set; }
    public DbSet<SyncFile> SyncFiles { get; set; }
    public DbSet<AzureCredential> AzureCredentials { get; set; }
    public DbSet<ConfigurationSetting> ConfigurationSettings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source=../hello.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AzureCredential>()
                    .ToTable("AzureCredential")
                    .HasKey("Name");

        modelBuilder.Entity<LocalFile>()
                    .ToTable("LocalFile")
                    .HasKey("Path");
        
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