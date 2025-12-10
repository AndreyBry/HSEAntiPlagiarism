using FileAnalysisService.Domain.Enums;

namespace FileAnalysisService.Domain.Entities;

public sealed class Report
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid WorkId { get; private set; }
    public ReportStatus Status { get; private set; }

    public bool IsPlagiarism { get; private set; }
    public string? Details { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    private Report() { }

    public Report(Guid workId, ReportStatus status, bool isPlagiarism, string? details, DateTime createdAtUtc)
    {
        WorkId = workId;
        Status = status;
        IsPlagiarism = isPlagiarism;
        Details = details;
        CreatedAtUtc = createdAtUtc;
    }
}
