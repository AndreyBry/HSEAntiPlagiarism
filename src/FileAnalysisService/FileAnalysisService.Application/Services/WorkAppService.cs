using FileAnalysisService.Abstractions.Interfaces;
using FileAnalysisService.Application.Dto;
using FileAnalysisService.Domain.Entities;
using FileAnalysisService.Domain.Enums;

namespace FileAnalysisService.Application.Services;

public sealed class WorkAppService
{
    private readonly IWorkRepository _works;
    private readonly IReportRepository _reports;
    private readonly IFileStoringClient _storing;
    private readonly IClock _clock;

    public WorkAppService(
        IWorkRepository works,
        IReportRepository reports,
        IFileStoringClient storing,
        IClock clock)
    {
        _works = works;
        _reports = reports;
        _storing = storing;
        _clock = clock;
    }

    public async Task<SubmitWorkResultDto> SubmitAsync(
        string studentId,
        string assignmentId,
        Guid fileId,
        CancellationToken ct)
    {
        var now = _clock.UtcNow;

        try
        {
            var meta = await _storing.GetMetaAsync(fileId, ct);
            var checksum = meta.Checksum;

            var work = new Work(studentId, assignmentId, fileId, checksum, now);
            await _works.AddAsync(work, ct);

            var earlier = await _works.FindEarlierWorkByChecksumAsync(
                assignmentId,
                checksum,
                now,
                ct);

            var isPlagiarism = earlier is not null && !string.Equals(earlier.StudentId, studentId, StringComparison.Ordinal);

            var details = isPlagiarism
                ? $"Earlier work found with same checksum by another student. EarlierWorkId={earlier!.Id}"
                : "No earlier matching work found.";

            var report = new Report(work.Id, ReportStatus.Completed, isPlagiarism, details, now);
            await _reports.AddAsync(report, ct);

            return new SubmitWorkResultDto(work.Id, ReportStatus.Completed.ToString(), isPlagiarism);
        }
        catch (Exception ex)
        {
            var fallbackWork = new Work(studentId, assignmentId, fileId, "unknown", now);
            await _works.AddAsync(fallbackWork, ct);

            var report = new Report(
                fallbackWork.Id,
                ReportStatus.Failed,
                false,
                $"Analysis failed: {ex.Message}",
                now);

            await _reports.AddAsync(report, ct);

            return new SubmitWorkResultDto(fallbackWork.Id, ReportStatus.Failed.ToString(), false);
        }
    }

    public async Task<IReadOnlyList<ReportItemDto>> GetReportsAsync(Guid workId, CancellationToken ct)
    {
        var items = await _reports.GetByWorkIdAsync(workId, ct);

        return items
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new ReportItemDto(
                x.Id,
                x.WorkId,
                x.Status.ToString(),
                x.IsPlagiarism,
                x.Details,
                x.CreatedAtUtc))
            .ToList();
    }
}
