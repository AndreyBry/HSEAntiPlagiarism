using BuildingBlocks.GrpcContracts.FileStoring;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;

namespace ApiGateway.Api.GrpcClients;

public sealed class FileStoringGrpcClient
{
    private readonly FileStoring.FileStoringClient _client;
    private readonly int _timeoutSeconds;

    public FileStoringGrpcClient(IConfiguration configuration)
    {
        var address = configuration["Grpc:FileStoring:Address"]
                      ?? throw new InvalidOperationException("Grpc:FileStoring:Address is not configured.");

        _timeoutSeconds = int.TryParse(configuration["Grpc:FileStoring:TimeoutSeconds"], out var t) ? t : 10;

        var channel = GrpcChannel.ForAddress(address);
        _client = new FileStoring.FileStoringClient(channel);
    }

    public async Task<UploadFileResponse> UploadAsync(
        string fileName,
        string contentType,
        byte[] data,
        CancellationToken ct)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(_timeoutSeconds));

        try
        {
            return await _client.UploadAsync(new UploadFileRequest
            {
                FileName = fileName,
                ContentType = contentType,
                Data = Google.Protobuf.ByteString.CopyFrom(data)
            }, cancellationToken: cts.Token);
        }
        catch (RpcException)
        {
            throw;
        }
    }
}
