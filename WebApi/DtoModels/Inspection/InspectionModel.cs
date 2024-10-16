using System.ComponentModel.DataAnnotations;
using WebApi.DtoModels.Diagnosis;
using WebApi.DtoModels.Doctor;
using WebApi.DtoModels.Enums;
using WebApi.DtoModels.Patient;

namespace WebApi.DtoModels.Inspection;

public class InspectionModel
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    public DateTime CreateTime { get; set; }
    public DateTime Date { get; set; }
    public string? Anamnesis { get; set; } = string.Empty;
    public string? Complaints { get; set; } = string.Empty;
    public string? Treatment { get; set; } = string.Empty;
    public Conclusion Conclusion { get; set; }
    public DateTime? NextVisitDate { get; set; }
    public DateTime? DeathDate { get; set; }
    public Guid? BaseInspectionId { get; set; }
    public Guid? PreviousInspectionId { get; set; }
    public PatientModel Patient { get; set; }
    public DoctorModel Doctor { get; set; }
    public List<DiagnosisModel>? Diagnoses { get; set; }
    public List<InspectionConsultationModel>? Consultations { get; set; }
}