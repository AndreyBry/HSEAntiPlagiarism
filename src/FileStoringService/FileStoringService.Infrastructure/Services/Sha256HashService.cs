using System.Security.Cryptography;
using System.Text;
using FileStoringService.Abstractions.Interfaces;

namespace FileStoringService.Infrastructure.Services;

public sealed class Sha256HashService : IHashService
{
    public string ComputeSha256(byte[] data)
    {
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(data);

        var sb = new StringBuilder(hash.Length * 2);
        foreach (var b in hash)
            sb.Append(b.ToString("x2"));

        return sb.ToString();
    }
}
