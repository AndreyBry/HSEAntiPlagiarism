using FileAnalysisService.Abstractions.Interfaces;
using Grpc.Core;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace FileAnalysisService.Infrastructure.Services;

public sealed class QuickChartWordCloudProvider : IWordCloudProvider
{
    private readonly HttpClient _http;

    public QuickChartWordCloudProvider(HttpClient http)
    {
        _http = http;
    }

    public async Task<byte[]> BuildAsync(string wordList, CancellationToken ct)
    {
        var requestBody = new
        {
            format = "png",
            width = 600,
            height = 400,
            fontScale = 15,
            scale = "linear",
            useWordList = true,
            text = wordList
        };

        var response = await _http.PostAsync(
            "https://quickchart.io/wordcloud",
            new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json"),
            ct);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsByteArrayAsync(ct);
    }
}
