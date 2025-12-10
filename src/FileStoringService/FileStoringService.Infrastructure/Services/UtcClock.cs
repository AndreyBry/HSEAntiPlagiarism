using FileStoringService.Abstractions.Interfaces;

namespace FileStoringService.Infrastructure.Services;

public sealed class UtcClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
