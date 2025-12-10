using FileAnalysisService.Abstractions.Interfaces;
using FileAnalysisService.Abstractions.Options;
using Microsoft.Extensions.Options;

namespace FileAnalysisService.Application.Services;

public sealed class WordCloudAppService
{
    private readonly IWorkRepository _works;
    private readonly IFileStoringClient _storing;
    private readonly ITextExtractor _extractor;
    private readonly IWordCloudProvider _provider;
    private readonly WordCloudOptions _options;

    public WordCloudAppService(
        IWorkRepository works,
        IFileStoringClient storing,
        ITextExtractor extractor,
        IWordCloudProvider provider,
        IOptions<WordCloudOptions> options)
    {
        _works = works;
        _storing = storing;
        _extractor = extractor;
        _provider = provider;
        _options = options.Value;
    }

    public async Task<byte[]> BuildForWorkAsync(Guid workId, CancellationToken ct)
    {
        var work = await _works.GetAsync(workId, ct)
                   ?? throw new InvalidOperationException($"Work not found: {workId}");

        var file = await _storing.DownloadAsync(work.FileId, ct);

        var text = _extractor.ExtractText(file.ContentType, file.Data);

        if (text.Length > _options.MaxTextLength)
            text = text[.._options.MaxTextLength];

        if (string.IsNullOrWhiteSpace(text))
            text = "empty";

        return await _provider.BuildAsync(text, ct);
    }
}
