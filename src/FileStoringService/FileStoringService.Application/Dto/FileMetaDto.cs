namespace FileStoringService.Application.Dto;

public sealed record FileMetaDto(
    Guid FileId,
    string FileName,
    string ContentType,
    long Size,
    string Checksum,
    string StorageKey,
    DateTime CreatedAtUtc);

public sealed record UploadResultDto(
    Guid FileId,
    string StorageKey,
    string Checksum,
    long Size);

public sealed record DownloadResultDto(
    string FileName,
    string ContentType,
    byte[] Data);
