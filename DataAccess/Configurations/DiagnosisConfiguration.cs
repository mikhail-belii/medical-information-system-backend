using Common.DbModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Configurations;

public class DiagnosisConfiguration : IEntityTypeConfiguration<DiagnosisEntity>
{
    public void Configure(EntityTypeBuilder<DiagnosisEntity> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Type).HasConversion<string>();
    }
}