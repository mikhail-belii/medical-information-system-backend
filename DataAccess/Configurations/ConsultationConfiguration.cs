using Common.DbModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Configurations;

public class ConsultationConfiguration : IEntityTypeConfiguration<ConsultationEntity>
{
    public void Configure(EntityTypeBuilder<ConsultationEntity> builder)
    {
        builder.HasKey(c => c.Id);
    }
}