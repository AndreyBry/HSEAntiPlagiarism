using FileAnalysisService.Domain.Entities;

namespace FileAnalysisService.Abstractions.Interfaces;

public interface IWorkRepository
{
    Task AddAsync(Work work, CancellationToken ct);
    Task<Work?> GetAsync(Guid id, CancellationToken ct);

    Task<Work?> FindEarlierWorkByChecksumAsync(
        string assignmentId,
        string checksum,
        DateTime submittedAtUtc,
        CancellationToken ct);
}
