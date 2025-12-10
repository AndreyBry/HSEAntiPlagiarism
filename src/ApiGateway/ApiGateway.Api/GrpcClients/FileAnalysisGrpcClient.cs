using BuildingBlocks.GrpcContracts.FileAnalysis;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;

namespace ApiGateway.Api.GrpcClients;

public sealed class FileAnalysisGrpcClient
{
    private readonly FileAnalysis.FileAnalysisClient _client;
    private readonly int _timeoutSeconds;

    public FileAnalysisGrpcClient(IConfiguration configuration)
    {
        var address = configuration["Grpc:FileAnalysis:Address"]
                      ?? throw new InvalidOperationException("Grpc:FileAnalysis:Address is not configured.");

        _timeoutSeconds = int.TryParse(configuration["Grpc:FileAnalysis:TimeoutSeconds"], out var t) ? t : 15;

        var channel = GrpcChannel.ForAddress(address);
        _client = new FileAnalysis.FileAnalysisClient(channel);
    }

    public async Task<SubmitWorkResponse> SubmitWorkAsync(
        string studentId,
        string assignmentId,
        string fileId,
        CancellationToken ct)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(_timeoutSeconds));

        return await _client.SubmitWorkAsync(new SubmitWorkRequest
        {
            StudentId = studentId,
            AssignmentId = assignmentId,
            FileId = fileId
        }, cancellationToken: cts.Token);
    }

    public async Task<GetReportsResponse> GetReportsAsync(
        string workId,
        CancellationToken ct)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(_timeoutSeconds));

        return await _client.GetReportsAsync(new GetReportsRequest
        {
            WorkId = workId
        }, cancellationToken: cts.Token);
    }

    public async Task<BuildWordCloudResponse> BuildWordCloudAsync(
        string workId,
        CancellationToken ct)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(_timeoutSeconds));

        return await _client.BuildWordCloudAsync(new BuildWordCloudRequest
        {
            WorkId = workId
        }, cancellationToken: cts.Token);
    }
}
