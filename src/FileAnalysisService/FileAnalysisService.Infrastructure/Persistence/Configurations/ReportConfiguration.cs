using FileAnalysisService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FileAnalysisService.Infrastructure.Persistence.Configurations;

public sealed class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.ToTable("reports");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.WorkId).IsRequired();
        builder.Property(x => x.Status).IsRequired();

        builder.Property(x => x.Details)
            .HasMaxLength(2000);

        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasIndex(x => x.WorkId);
    }
}
