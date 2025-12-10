using FileAnalysisService.Abstractions.Interfaces;

namespace FileAnalysisService.Infrastructure.Services;

public sealed class UtcClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
