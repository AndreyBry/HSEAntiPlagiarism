using Amazon.S3;
using FileStoringService.Abstractions.Interfaces;
using FileStoringService.Abstractions.Options;
using FileStoringService.Application.Services;
using FileStoringService.Infrastructure.Persistence;
using FileStoringService.Infrastructure.Repositories;
using FileStoringService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Internal;

namespace FileStoringService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddFileStoringInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<S3Options>()
            .Bind(configuration.GetSection(S3Options.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddDbContext<StoringDbContext>(opt =>
        {
            var cs = configuration.GetConnectionString("StoringDb");
            opt.UseNpgsql(cs);
        });

        services.AddScoped<IFileMetaRepository, FileMetaRepository>();
        services.AddSingleton<IHashService, Sha256HashService>();
        services.AddSingleton<IClock, UtcClock>();

        services.AddSingleton<IAmazonS3>(sp =>
        {
            var s3 = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<S3Options>>().Value;

            var config = new AmazonS3Config
            {
                ServiceURL = s3.Endpoint,
                ForcePathStyle = true,
                UseHttp = !s3.UseSsl
            };

            return new AmazonS3Client(s3.AccessKey, s3.SecretKey, config);
        });

        services.AddScoped<IFileStorage, S3FileStorage>();

        services.AddScoped<FileStorageAppService>();

        return services;
    }

    public static async Task ApplyStoringMigrationsAsync(this IServiceProvider provider)
    {
        var db = provider.GetRequiredService<StoringDbContext>();
        await db.Database.MigrateAsync();
    }
}
