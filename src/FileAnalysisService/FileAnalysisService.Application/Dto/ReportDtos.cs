namespace FileAnalysisService.Application.Dto;

public sealed record ReportItemDto(
    Guid ReportId,
    Guid WorkId,
    string Status,
    bool IsPlagiarism,
    string? Details,
    DateTime CreatedAtUtc);
