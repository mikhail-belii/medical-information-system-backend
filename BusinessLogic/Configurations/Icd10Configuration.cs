using Common.DbModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusinessLogic.Configurations;

public class Icd10Configuration : IEntityTypeConfiguration<Icd10Entity>
{
    public void Configure(EntityTypeBuilder<Icd10Entity> builder)
    {
        builder.HasKey(i => i.Id);
    }
}