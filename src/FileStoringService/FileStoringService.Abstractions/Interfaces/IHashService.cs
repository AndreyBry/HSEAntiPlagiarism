namespace FileStoringService.Abstractions.Interfaces;

public interface IHashService
{
    string ComputeSha256(byte[] data);
}
