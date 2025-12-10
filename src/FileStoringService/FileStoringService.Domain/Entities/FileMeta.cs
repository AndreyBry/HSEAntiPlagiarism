namespace FileStoringService.Domain.Entities;

public sealed class FileMeta
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public string FileName { get; private set; } = default!;
    public string ContentType { get; private set; } = "application/octet-stream";
    public long Size { get; private set; }
    public string Checksum { get; private set; } = default!;
    public string StorageKey { get; private set; } = default!;
    public DateTime CreatedAtUtc { get; private set; }

    private FileMeta() { }

    public FileMeta(
        string fileName,
        string contentType,
        long size,
        string checksum,
        string storageKey,
        DateTime createdAtUtc)
    {
        FileName = string.IsNullOrWhiteSpace(fileName) ? "file" : fileName;
        ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType;
        Size = size < 0 ? 0 : size;
        Checksum = checksum;
        StorageKey = storageKey;
        CreatedAtUtc = createdAtUtc;
    }
}
