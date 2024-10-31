using BusinessLogic.Configurations;
using Common.DbModels;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<DoctorEntity> Doctors { get; set; }
    public DbSet<PatientEntity> Patients { get; set; }
    public DbSet<InspectionEntity> Inspections { get; set; }
    public DbSet<DiagnosisEntity> Diagnoses { get; set; }
    public DbSet<ConsultationEntity> Consultations { get; set; }
    public DbSet<Icd10Entity> Icd10s { get; set; }
    public DbSet<SpecialityEntity> Specialities { get; set; }
    public DbSet<CommentEntity> Comments { get; set; }
    public DbSet<NotificationLog> NotificationLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CommentConfiguration());
        modelBuilder.ApplyConfiguration(new ConsultationConfiguration());
        modelBuilder.ApplyConfiguration(new DiagnosisConfiguration());
        modelBuilder.ApplyConfiguration(new DoctorConfiguration());
        modelBuilder.ApplyConfiguration(new Icd10Configuration());
        modelBuilder.ApplyConfiguration(new InspectionConfiguration());
        modelBuilder.ApplyConfiguration(new PatientConfiguration());
        modelBuilder.ApplyConfiguration(new SpecialityConfiguration());
        
        base.OnModelCreating(modelBuilder);
    }
}