using Amazon.S3;
using Amazon.S3.Model;
using FileStoringService.Abstractions.Interfaces;
using FileStoringService.Abstractions.Options;
using Microsoft.Extensions.Options;

namespace FileStoringService.Infrastructure.Services;

public sealed class S3FileStorage : IFileStorage
{
    private readonly IAmazonS3 _s3;
    private readonly S3Options _options;

    public S3FileStorage(IAmazonS3 s3, IOptions<S3Options> options)
    {
        _s3 = s3;
        _options = options.Value;
    }

    public async Task<string> UploadAsync(
        string fileName,
        string contentType,
        byte[] data,
        CancellationToken ct)
    {
        await EnsureBucketExistsAsync(ct);

        var safeName = string.IsNullOrWhiteSpace(fileName) ? "file" : fileName.Trim();
        var key = $"{Guid.NewGuid():N}_{safeName}";

        using var ms = new MemoryStream(data);

        var request = new PutObjectRequest
        {
            BucketName = _options.Bucket,
            Key = key,
            InputStream = ms,
            ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType
        };

        await _s3.PutObjectAsync(request, ct);

        return key;
    }

    public async Task<byte[]> DownloadAsync(string storageKey, CancellationToken ct)
    {
        var request = new GetObjectRequest
        {
            BucketName = _options.Bucket,
            Key = storageKey
        };

        using var response = await _s3.GetObjectAsync(request, ct);
        using var ms = new MemoryStream();
        await response.ResponseStream.CopyToAsync(ms, ct);
        return ms.ToArray();
    }

    private async Task EnsureBucketExistsAsync(CancellationToken ct)
    {
        var buckets = await _s3.ListBucketsAsync(ct);
        if (buckets.Buckets.Any(b => b.BucketName == _options.Bucket))
            return;

        await _s3.PutBucketAsync(new PutBucketRequest
        {
            BucketName = _options.Bucket
        }, ct);
    }
}
