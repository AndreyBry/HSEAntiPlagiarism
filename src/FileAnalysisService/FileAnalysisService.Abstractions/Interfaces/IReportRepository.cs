using FileAnalysisService.Domain.Entities;

namespace FileAnalysisService.Abstractions.Interfaces;

public interface IReportRepository
{
    Task AddAsync(Report report, CancellationToken ct);
    Task<IReadOnlyList<Report>> GetByWorkIdAsync(Guid workId, CancellationToken ct);
}
