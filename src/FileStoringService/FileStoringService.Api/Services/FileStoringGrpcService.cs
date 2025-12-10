using BuildingBlocks.GrpcContracts.FileStoring;
using FileStoringService.Application.Services;
using Grpc.Core;

namespace FileStoringService.Api.Services;

public sealed class FileStoringGrpcService : FileStoring.FileStoringBase
{
    private readonly FileStorageAppService _app;

    public FileStoringGrpcService(FileStorageAppService app)
    {
        _app = app;
    }

    public override async Task<UploadFileResponse> Upload(UploadFileRequest request, ServerCallContext context)
    {
        if (request.Data is null || request.Data.Length == 0)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "File data is empty."));

        var result = await _app.UploadAsync(
            request.FileName,
            request.ContentType,
            request.Data.ToByteArray(),
            context.CancellationToken);

        return new UploadFileResponse
        {
            FileId = result.FileId.ToString(),
            StorageKey = result.StorageKey,
            Checksum = result.Checksum,
            Size = result.Size
        };
    }

    public override async Task<DownloadFileResponse> Download(DownloadFileRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.FileId, out var fileId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid fileId."));

        var file = await _app.DownloadAsync(fileId, context.CancellationToken);

        return new DownloadFileResponse
        {
            FileName = file.FileName,
            ContentType = file.ContentType,
            Data = Google.Protobuf.ByteString.CopyFrom(file.Data)
        };
    }

    public override async Task<GetMetaResponse> GetMeta(GetMetaRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.FileId, out var fileId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid fileId."));

        var meta = await _app.GetMetaAsync(fileId, context.CancellationToken);

        return new GetMetaResponse
        {
            FileId = meta.FileId.ToString(),
            FileName = meta.FileName,
            ContentType = meta.ContentType,
            Checksum = meta.Checksum,
            Size = meta.Size,
            StorageKey = meta.StorageKey,
            CreatedAtUtc = meta.CreatedAtUtc.ToString("O")
        };
    }
}
