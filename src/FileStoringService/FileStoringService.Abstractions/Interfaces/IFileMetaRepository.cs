using FileStoringService.Domain.Entities;

namespace FileStoringService.Abstractions.Interfaces;

public interface IFileMetaRepository
{
    Task AddAsync(FileMeta meta, CancellationToken ct);
    Task<FileMeta?> GetAsync(Guid id, CancellationToken ct);
}
