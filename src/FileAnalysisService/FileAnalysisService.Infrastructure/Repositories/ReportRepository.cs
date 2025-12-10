using FileAnalysisService.Abstractions.Interfaces;
using FileAnalysisService.Domain.Entities;
using FileAnalysisService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FileAnalysisService.Infrastructure.Repositories;

public sealed class ReportRepository : IReportRepository
{
    private readonly AnalysisDbContext _db;

    public ReportRepository(AnalysisDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Report report, CancellationToken ct)
    {
        _db.Reports.Add(report);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Report>> GetByWorkIdAsync(Guid workId, CancellationToken ct)
        => await _db.Reports
            .Where(x => x.WorkId == workId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(ct);
}
