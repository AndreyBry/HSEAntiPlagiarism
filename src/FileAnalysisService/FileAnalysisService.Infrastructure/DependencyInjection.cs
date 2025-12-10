using FileAnalysisService.Abstractions.Interfaces;
using FileAnalysisService.Abstractions.Options;
using FileAnalysisService.Application.Services;
using FileAnalysisService.Infrastructure.Persistence;
using FileAnalysisService.Infrastructure.Repositories;
using FileAnalysisService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FileAnalysisService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddFileAnalysisInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<WordCloudOptions>()
            .Bind(configuration.GetSection(WordCloudOptions.SectionName))
            .Validate(o => o.MaxTextLength > 0 && o.MaxTextLength <= 50000, "Invalid WordCloud options.")
            .ValidateOnStart();

        services.AddDbContext<AnalysisDbContext>(opt =>
        {
            var cs = configuration.GetConnectionString("AnalysisDb");
            opt.UseNpgsql(cs);
        });

        services.AddScoped<IWorkRepository, WorkRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();

        services.AddSingleton<IHashService, Sha256HashService>();
        services.AddSingleton<IClock, UtcClock>();
        services.AddSingleton<ITextExtractor, Utf8TextExtractor>();

        services.AddScoped<IFileStoringClient, FileStoringGrpcClient>();

        services.AddHttpClient<IWordCloudProvider, QuickChartWordCloudProvider>();

        services.AddScoped<WorkAppService>();
        services.AddScoped<WordCloudAppService>();

        return services;
    }

    public static async Task ApplyAnalysisMigrationsAsync(this IServiceProvider provider)
    {
        var db = provider.GetRequiredService<AnalysisDbContext>();
        await db.Database.MigrateAsync();
    }
}
