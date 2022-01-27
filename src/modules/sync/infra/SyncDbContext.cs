using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace azsync;

public class SyncDbContext : DbContext, IUnitOfWork
{
    public DbSet<LocalFile> LocalFiles { get; set; }
    public DbSet<SyncFile> SyncFiles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source=hello.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LocalFile>()
                    .ToTable("LocalFile")
                    .HasKey("Path");
        
        modelBuilder.Entity<SyncFile>()
                    .Property(e => e.LocalFilePathHash).HasColumnName("LocalFilePathHash");

        modelBuilder.Entity<SyncFile>()
                    .ToTable("SyncFile")
                    .HasKey("Id");
    }

    void IUnitOfWork.SaveChanges()
    {
        base.SaveChanges();
    }
}