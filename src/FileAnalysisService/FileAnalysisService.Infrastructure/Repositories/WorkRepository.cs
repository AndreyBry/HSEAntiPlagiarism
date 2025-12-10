using FileAnalysisService.Abstractions.Interfaces;
using FileAnalysisService.Domain.Entities;
using FileAnalysisService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FileAnalysisService.Infrastructure.Repositories;

public sealed class WorkRepository : IWorkRepository
{
    private readonly AnalysisDbContext _db;

    public WorkRepository(AnalysisDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Work work, CancellationToken ct)
    {
        _db.Works.Add(work);
        await _db.SaveChangesAsync(ct);
    }

    public Task<Work?> GetAsync(Guid id, CancellationToken ct)
        => _db.Works.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<Work?> FindEarlierWorkByChecksumAsync(
        string assignmentId,
        string checksum,
        DateTime submittedAtUtc,
        CancellationToken ct)
        => _db.Works
            .Where(x => x.AssignmentId == assignmentId
                     && x.Checksum == checksum
                     && x.SubmittedAtUtc < submittedAtUtc)
            .OrderBy(x => x.SubmittedAtUtc)
            .FirstOrDefaultAsync(ct);
}
