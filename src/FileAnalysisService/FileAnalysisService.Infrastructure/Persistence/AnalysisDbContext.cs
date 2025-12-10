using FileAnalysisService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FileAnalysisService.Infrastructure.Persistence;

public sealed class AnalysisDbContext : DbContext
{
    public DbSet<Work> Works => Set<Work>();
    public DbSet<Report> Reports => Set<Report>();

    public AnalysisDbContext(DbContextOptions<AnalysisDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AnalysisDbContext).Assembly);
    }
}
