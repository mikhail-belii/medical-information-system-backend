using Common.DbModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Configurations;

public class InspectionConfiguration : IEntityTypeConfiguration<InspectionEntity>
{
    public void Configure(EntityTypeBuilder<InspectionEntity> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Conclusion).HasConversion<string>();
        builder.HasOne(i => i.Patient);
    }
}