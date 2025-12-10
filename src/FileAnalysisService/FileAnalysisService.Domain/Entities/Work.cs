namespace FileAnalysisService.Domain.Entities;

public sealed class Work
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public string StudentId { get; private set; } = default!;
    public string AssignmentId { get; private set; } = default!;
    public Guid FileId { get; private set; }

    public string Checksum { get; private set; } = default!;
    public DateTime SubmittedAtUtc { get; private set; }

    private Work() { }

    public Work(string studentId, string assignmentId, Guid fileId, string checksum, DateTime submittedAtUtc)
    {
        StudentId = studentId;
        AssignmentId = assignmentId;
        FileId = fileId;
        Checksum = checksum;
        SubmittedAtUtc = submittedAtUtc;
    }
}
