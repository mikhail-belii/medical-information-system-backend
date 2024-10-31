using Common.DbModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusinessLogic.Configurations;

public class DoctorConfiguration : IEntityTypeConfiguration<DoctorEntity>
{
    public void Configure(EntityTypeBuilder<DoctorEntity> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Gender).HasConversion<string>();
        builder.HasMany(d => d.Inspections)
            .WithOne(i => i.Doctor);
    }
}