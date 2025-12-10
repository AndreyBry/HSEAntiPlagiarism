using BuildingBlocks.GrpcContracts.FileAnalysis;
using FileAnalysisService.Application.Services;
using Grpc.Core;

namespace FileAnalysisService.Api.Services;

public sealed class FileAnalysisGrpcService : FileAnalysis.FileAnalysisBase
{
    private readonly WorkAppService _workApp;
    private readonly WordCloudAppService _cloudApp;

    public FileAnalysisGrpcService(WorkAppService workApp, WordCloudAppService cloudApp)
    {
        _workApp = workApp;
        _cloudApp = cloudApp;
    }

    public override async Task<SubmitWorkResponse> SubmitWork(SubmitWorkRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.FileId, out var fileId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid fileId."));

        var result = await _workApp.SubmitAsync(
            request.StudentId,
            request.AssignmentId,
            fileId,
            context.CancellationToken);

        return new SubmitWorkResponse
        {
            WorkId = result.WorkId.ToString(),
            Status = result.Status,
            IsPlagiarism = result.IsPlagiarism
        };
    }

    public override async Task<GetReportsResponse> GetReports(GetReportsRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.WorkId, out var workId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid workId."));

        try
        {
            var reports = await _workApp.GetReportsAsync(workId, context.CancellationToken);

            var response = new GetReportsResponse();
            response.Reports.AddRange(reports.Select(r => new ReportDto
            {
                ReportId = r.ReportId.ToString(),
                WorkId = r.WorkId.ToString(),
                Status = r.Status,
                IsPlagiarism = r.IsPlagiarism,
                Details = r.Details ?? string.Empty,
                CreatedAtUtc = r.CreatedAtUtc.ToString("O")
            }));

            return response;
        }
        catch (InvalidOperationException ex) when (
            ex.Message.StartsWith("Work not found", StringComparison.OrdinalIgnoreCase))
        {
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
    }

    public override async Task<BuildWordCloudResponse> BuildWordCloud(BuildWordCloudRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.WorkId, out var workId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid workId."));

        try
        {
            var bytes = await _cloudApp.BuildForWorkAsync(workId, context.CancellationToken);

            return new BuildWordCloudResponse
            {
                ImagePng = Google.Protobuf.ByteString.CopyFrom(bytes)
            };
        }
        catch (InvalidOperationException ex) when (
            ex.Message.StartsWith("Work not found", StringComparison.OrdinalIgnoreCase))
        {
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
    }
}
