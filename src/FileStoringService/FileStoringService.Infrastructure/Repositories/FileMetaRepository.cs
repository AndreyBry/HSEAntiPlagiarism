using FileStoringService.Abstractions.Interfaces;
using FileStoringService.Domain.Entities;
using FileStoringService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FileStoringService.Infrastructure.Repositories;

public sealed class FileMetaRepository : IFileMetaRepository
{
    private readonly StoringDbContext _db;

    public FileMetaRepository(StoringDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(FileMeta meta, CancellationToken ct)
    {
        _db.Files.Add(meta);
        await _db.SaveChangesAsync(ct);
    }

    public Task<FileMeta?> GetAsync(Guid id, CancellationToken ct)
        => _db.Files.FirstOrDefaultAsync(x => x.Id == id, ct);
}
