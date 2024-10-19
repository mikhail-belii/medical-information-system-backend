using Common.DbModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Configurations;

public class SpecialityConfiguration : IEntityTypeConfiguration<SpecialityEntity>
{
    public void Configure(EntityTypeBuilder<SpecialityEntity> builder)
    {
        builder.HasKey(s => s.Id);
    }
}