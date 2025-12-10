using System.Text;
using FileAnalysisService.Abstractions.Interfaces;

namespace FileAnalysisService.Infrastructure.Services;

public sealed class Utf8TextExtractor : ITextExtractor
{
    public string ExtractText(string contentType, byte[] data)
    {
        try
        {
            return Encoding.UTF8.GetString(data);
        }
        catch
        {
            return string.Empty;
        }
    }
}
