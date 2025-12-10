namespace FileAnalysisService.Abstractions.Interfaces;

public interface IClock
{
    DateTime UtcNow { get; }
}
