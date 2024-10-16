using System.ComponentModel.DataAnnotations;
using WebApi.DtoModels.Diagnosis;
using WebApi.DtoModels.Enums;

namespace WebApi.DtoModels.Inspection;

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