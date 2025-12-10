namespace ApiGateway.Api.Contracts;

public sealed class SubmitWorkRequestModel
{
    public string StudentId { get; init; } = default!;
    public string AssignmentId { get; init; } = default!;
    public IFormFile File { get; init; } = default!;
}
