using System.ComponentModel.DataAnnotations;
using Common.DtoModels.Diagnosis;
using Common.Enums;

namespace Common.DtoModels.Inspection;

public class InspectionEditModel
{
    [MaxLength(5000)]
    public string? Anamnesis { get; set; }
    [Required]
    [MinLength(1)]
    [MaxLength(5000)]
    public string Complaints { get; set; }
    [Required]
    [MinLength(1)]
    [MaxLength(5000)]
    public string Treatment { get; set; }
    [Required]
    public Conclusion Conclusion { get; set; }
    public DateTime? NextVisitDate { get; set; }
    public DateTime? DeathDate { get; set; }
    [Required]
    [MinLength(1)]
    public List<DiagnosisCreateModel> Diagnoses { get; set; }
}