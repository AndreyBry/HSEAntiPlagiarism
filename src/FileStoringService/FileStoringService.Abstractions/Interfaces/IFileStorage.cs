namespace FileStoringService.Abstractions.Interfaces;

public interface IFileStorage
{
    Task<string> UploadAsync(
        string fileName,
        string contentType,
        byte[] data,
        CancellationToken ct);

    Task<byte[]> DownloadAsync(string storageKey, CancellationToken ct);
}
