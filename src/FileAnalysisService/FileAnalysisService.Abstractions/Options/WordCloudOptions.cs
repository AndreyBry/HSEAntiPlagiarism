namespace FileAnalysisService.Abstractions.Options;

public sealed class WordCloudOptions
{
    public const string SectionName = "WordCloud";

    public int MaxTextLength { get; init; } = 5000;
}
