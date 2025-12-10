namespace FileStoringService.Abstractions.Options;

public sealed class S3Options
{
    public const string SectionName = "S3";

    public string Endpoint { get; init; } = default!;
    public string AccessKey { get; init; } = default!;
    public string SecretKey { get; init; } = default!;
    public string Bucket { get; init; } = default!;
    public bool UseSsl { get; init; }
    public string Region { get; init; } = "us-east-1";
}
