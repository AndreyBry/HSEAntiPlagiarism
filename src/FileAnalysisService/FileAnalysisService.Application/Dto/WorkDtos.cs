namespace FileAnalysisService.Application.Dto;

public sealed record SubmitWorkResultDto(
    Guid WorkId,
    string Status,
    bool IsPlagiarism);
