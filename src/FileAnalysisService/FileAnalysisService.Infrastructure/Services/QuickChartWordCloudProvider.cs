using System.Net;
using FileAnalysisService.Abstractions.Interfaces;

namespace FileAnalysisService.Infrastructure.Services;

public sealed class QuickChartWordCloudProvider : IWordCloudProvider
{
    private readonly HttpClient _http;

    public QuickChartWordCloudProvider(HttpClient http)
    {
        _http = http;
    }

    public async Task<byte[]> BuildAsync(string text, CancellationToken ct)
    {
        var encoded = WebUtility.UrlEncode(text);

        var url = $"https://quickchart.io/wordcloud?format=png&width=600&height=400&text={encoded}";

        using var response = await _http.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsByteArrayAsync(ct);
    }
}
