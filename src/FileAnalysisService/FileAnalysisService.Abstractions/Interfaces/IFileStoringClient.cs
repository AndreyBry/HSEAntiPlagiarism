namespace FileAnalysisService.Abstractions.Interfaces;

public interface IFileStoringClient
{
    Task<(string FileName, string ContentType, byte[] Data)> DownloadAsync(Guid fileId, CancellationToken ct);
    Task<(string Checksum, long Size)> GetMetaAsync(Guid fileId, CancellationToken ct);
}
