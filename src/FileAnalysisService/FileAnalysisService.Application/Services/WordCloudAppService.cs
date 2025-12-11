using FileAnalysisService.Abstractions.Interfaces;
using FileAnalysisService.Abstractions.Options;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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

    private static readonly Regex WordRegex = new(@"[\p{L}\p{M}]{2,}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly HashSet<string> StopWords =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "и","а","но","что","это","как","так","же","бы","в","во","на","к","с","со","у","от","до","для","по","из",
            "the","and","or","to","of","in","on","for","with","is","are","was","were","be","as","at","by"
        };

    public static Dictionary<string, int> CountWords(string? text)
    {
        var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (Match m in WordRegex.Matches(text ?? string.Empty))
        {
            var w = m.Value.Trim().ToLowerInvariant();
            if (w.Length < 2) continue;
            if (StopWords.Contains(w)) continue;

            dict.TryGetValue(w, out var c);
            dict[w] = c + 1;
        }

        return dict;
    }

    public static string BuildWordList(Dictionary<string, int> freq, int take = 80)
    {
        var top = freq
            .OrderByDescending(x => x.Value)
            .ThenBy(x => x.Key)
            .Take(take)
            .ToArray();

        var sb = new StringBuilder();
        for (var i = 0; i < top.Length; i++)
        {
            var (word, count) = top[i];

            if (i > 0)
                sb.Append(',');

            var safeWord = word.Replace(",", " ").Replace(":", " ");

            sb.Append(safeWord);
            sb.Append(':');
            sb.Append(count);
        }

        return sb.ToString();
    }

    public async Task<byte[]> BuildForWorkAsync(Guid workId, CancellationToken ct)
    {
        var work = await _works.GetAsync(workId, ct)
                   ?? throw new InvalidOperationException($"Work not found: {workId}");

        var file = await _storing.DownloadAsync(work.FileId, ct);

        var text = _extractor.ExtractText(file.ContentType, file.Data);

        var freq = CountWords(text);

        if (freq.Count == 0)
        {
            return Array.Empty<byte>();
        }

        var wordList = BuildWordList(freq, take: 80);

        return await _provider.BuildAsync(wordList, ct);
    }
}
