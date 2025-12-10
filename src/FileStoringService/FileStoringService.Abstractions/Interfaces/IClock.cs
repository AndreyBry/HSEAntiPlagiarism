namespace FileStoringService.Abstractions.Interfaces;

public interface IClock
{
    DateTime UtcNow { get; }
}
