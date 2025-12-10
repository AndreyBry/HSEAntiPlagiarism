namespace FileAnalysisService.Abstractions.Interfaces;

public interface IWordCloudProvider
{
    Task<byte[]> BuildAsync(string text, CancellationToken ct);
}
