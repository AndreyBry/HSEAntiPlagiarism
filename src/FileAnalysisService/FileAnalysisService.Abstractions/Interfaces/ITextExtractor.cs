namespace FileAnalysisService.Abstractions.Interfaces;

public interface ITextExtractor
{
    string ExtractText(string contentType, byte[] data);
}
