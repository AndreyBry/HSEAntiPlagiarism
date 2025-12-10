using FileStoringService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FileStoringService.Infrastructure.Persistence;

public sealed class StoringDbContext : DbContext
{
    public DbSet<FileMeta> Files => Set<FileMeta>();

    public StoringDbContext(DbContextOptions<StoringDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StoringDbContext).Assembly);
    }
}
