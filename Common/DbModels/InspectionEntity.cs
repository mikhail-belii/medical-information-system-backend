using Common.Enums;

namespace Common.DbModels;

public class InspectionEntity
{
    public Guid Id { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime? Date { get; set; }
    public string? Anamnesis { get; set; }
    public string? Complaints { get; set; }
    public string? Treatment { get; set; }
    public Conclusion? Conclusion { get; set; }
    public DateTime? NextVisitDate { get; set; }
    public DateTime? DeathDate { get; set; }
    public Guid? BaseInspectionId { get; set; }
    public Guid? PreviousInspectionId { get; set; }
    public PatientEntity? Patient { get; set; }
    public DoctorEntity? Doctor { get; set; }
    public List<DiagnosisEntity>? Diagnoses { get; set; }
    public List<ConsultationEntity>? Consultations { get; set; }
}