using ApiGateway.Api.Contracts;
using ApiGateway.Api.GrpcClients;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;

using GrpcStatusCode = Grpc.Core.StatusCode;

namespace ApiGateway.Api.Controllers;

[ApiController]
[Route("works")]
public sealed class WorksController : ControllerBase
{
    private readonly FileStoringGrpcClient _storing;
    private readonly FileAnalysisGrpcClient _analysis;
    private readonly ILogger<WorksController> _logger;

    public WorksController(
        FileStoringGrpcClient storing,
        FileAnalysisGrpcClient analysis,
        ILogger<WorksController> logger)
    {
        _storing = storing;
        _analysis = analysis;
        _logger = logger;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Submit([FromForm] SubmitWorkRequestModel model, CancellationToken ct)
    {
        if (model.File is null || model.File.Length == 0)
            return BadRequest("File is required.");

        if (string.IsNullOrWhiteSpace(model.StudentId))
            return BadRequest("StudentId is required.");

        if (string.IsNullOrWhiteSpace(model.AssignmentId))
            return BadRequest("AssignmentId is required.");

        byte[] data;
        await using (var ms = new MemoryStream())
        {
            await model.File.CopyToAsync(ms, ct);
            data = ms.ToArray();
        }

        try
        {
            var upload = await _storing.UploadAsync(
                model.File.FileName,
                model.File.ContentType ?? "application/octet-stream",
                data,
                ct);

            var submit = await _analysis.SubmitWorkAsync(
                model.StudentId,
                model.AssignmentId,
                upload.FileId,
                ct);

            return Ok(new
            {
                workId = submit.WorkId,
                status = submit.Status,
                isPlagiarism = submit.IsPlagiarism,
                fileId = upload.FileId,
                checksum = upload.Checksum,
                size = upload.Size
            });
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "gRPC error during Submit.");

            return ex.StatusCode switch
            {
                GrpcStatusCode.Unavailable => StatusCode(503, "Service unavailable."),
                GrpcStatusCode.DeadlineExceeded => StatusCode(504, "Service timeout."),
                GrpcStatusCode.InvalidArgument => BadRequest(ex.Status.Detail),
                _ => StatusCode(500, "Unexpected gRPC error.")
            };
        }
    }

    [HttpGet("{workId}/reports")]
    public async Task<IActionResult> GetReports([FromRoute] string workId, CancellationToken ct)
    {
        try
        {
            var reports = await _analysis.GetReportsAsync(workId, ct);

            return Ok(reports.Reports.Select(r => new
            {
                reportId = r.ReportId,
                workId = r.WorkId,
                status = r.Status,
                isPlagiarism = r.IsPlagiarism,
                details = r.Details,
                createdAtUtc = r.CreatedAtUtc
            }));
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "gRPC error during GetReports.");

            return ex.StatusCode switch
            {
                GrpcStatusCode.NotFound => NotFound(ex.Status.Detail),
                GrpcStatusCode.Unavailable => StatusCode(503, "Service unavailable."),
                GrpcStatusCode.DeadlineExceeded => StatusCode(504, "Service timeout."),
                GrpcStatusCode.InvalidArgument => BadRequest(ex.Status.Detail),
                _ => StatusCode(500, "Unexpected gRPC error.")
            };
        }
    }

    [HttpGet("{workId}/wordcloud")]
    public async Task<IActionResult> GetWordCloud([FromRoute] string workId, CancellationToken ct)
    {
        try
        {
            var res = await _analysis.BuildWordCloudAsync(workId, ct);

            var bytes = res.ImagePng.ToByteArray();
            if (bytes.Length == 0)
                return NotFound("Word cloud is empty.");

            return File(bytes, "image/png");
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "gRPC error during GetWordCloud.");

            return ex.StatusCode switch
            {
                GrpcStatusCode.NotFound => NotFound(ex.Status.Detail),
                GrpcStatusCode.Unavailable => StatusCode(503, "Service unavailable."),
                GrpcStatusCode.DeadlineExceeded => StatusCode(504, "Service timeout."),
                GrpcStatusCode.InvalidArgument => BadRequest(ex.Status.Detail),
                _ => StatusCode(500, "Unexpected gRPC error.")
            };
        }
    }
}
