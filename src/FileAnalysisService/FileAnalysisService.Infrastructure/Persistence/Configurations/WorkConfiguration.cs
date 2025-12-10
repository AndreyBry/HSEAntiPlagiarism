using FileAnalysisService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FileAnalysisService.Infrastructure.Persistence.Configurations;

public sealed class WorkConfiguration : IEntityTypeConfiguration<Work>
{
    public void Configure(EntityTypeBuilder<Work> builder)
    {
        builder.ToTable("works");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.StudentId)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.AssignmentId)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.Checksum)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.SubmittedAtUtc)
            .IsRequired();

        builder.Property(x => x.FileId)
            .IsRequired();

        builder.HasIndex(x => new { x.AssignmentId, x.Checksum, x.SubmittedAtUtc });
    }
}
