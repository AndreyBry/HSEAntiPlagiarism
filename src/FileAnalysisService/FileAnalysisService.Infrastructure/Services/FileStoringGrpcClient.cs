using FileAnalysisService.Abstractions.Interfaces;
using BuildingBlocks.GrpcContracts.FileStoring;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;

namespace FileAnalysisService.Infrastructure.Services;

public sealed class FileStoringGrpcClient : IFileStoringClient
{
    private readonly FileStoring.FileStoringClient _client;

    public FileStoringGrpcClient(IConfiguration configuration)
    {
        var address = configuration["Grpc:FileStoring:Address"]
                      ?? throw new InvalidOperationException("Grpc:FileStoring:Address is not configured.");

        var channel = GrpcChannel.ForAddress(address);
        _client = new FileStoring.FileStoringClient(channel);
    }

    public async Task<(string FileName, string ContentType, byte[] Data)> DownloadAsync(Guid fileId, CancellationToken ct)
    {
        var response = await _client.DownloadAsync(new DownloadFileRequest
        {
            FileId = fileId.ToString()
        }, cancellationToken: ct);

        return (response.FileName, response.ContentType, response.Data.ToByteArray());
    }

    public async Task<(string Checksum, long Size)> GetMetaAsync(Guid fileId, CancellationToken ct)
    {
        var response = await _client.GetMetaAsync(new GetMetaRequest
        {
            FileId = fileId.ToString()
        }, cancellationToken: ct);

        return (response.Checksum, response.Size);
    }
}
