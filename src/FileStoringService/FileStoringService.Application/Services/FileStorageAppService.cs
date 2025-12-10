using FileStoringService.Abstractions.Interfaces;
using FileStoringService.Application.Dto;
using FileStoringService.Domain.Entities;

namespace FileStoringService.Application.Services;

public sealed class FileStorageAppService
{
    private readonly IFileStorage _storage;
    private readonly IFileMetaRepository _metaRepository;
    private readonly IHashService _hash;
    private readonly IClock _clock;

    public FileStorageAppService(
        IFileStorage storage,
        IFileMetaRepository metaRepository,
        IHashService hash,
        IClock clock)
    {
        _storage = storage;
        _metaRepository = metaRepository;
        _hash = hash;
        _clock = clock;
    }

    public async Task<UploadResultDto> UploadAsync(
        string fileName,
        string contentType,
        byte[] data,
        CancellationToken ct)
    {
        var checksum = _hash.ComputeSha256(data);
        var storageKey = await _storage.UploadAsync(fileName, contentType, data, ct);

        var meta = new FileMeta(
            fileName,
            contentType,
            data.LongLength,
            checksum,
            storageKey,
            _clock.UtcNow);

        await _metaRepository.AddAsync(meta, ct);

        return new UploadResultDto(meta.Id, storageKey, checksum, meta.Size);
    }

    public async Task<DownloadResultDto> DownloadAsync(Guid fileId, CancellationToken ct)
    {
        var meta = await _metaRepository.GetAsync(fileId, ct);
        if (meta is null)
            throw new InvalidOperationException($"File meta not found: {fileId}");

        var data = await _storage.DownloadAsync(meta.StorageKey, ct);

        return new DownloadResultDto(meta.FileName, meta.ContentType, data);
    }

    public async Task<FileMetaDto> GetMetaAsync(Guid fileId, CancellationToken ct)
    {
        var meta = await _metaRepository.GetAsync(fileId, ct);
        if (meta is null)
            throw new InvalidOperationException($"File meta not found: {fileId}");

        return new FileMetaDto(
            meta.Id,
            meta.FileName,
            meta.ContentType,
            meta.Size,
            meta.Checksum,
            meta.StorageKey,
            meta.CreatedAtUtc);
    }
}
