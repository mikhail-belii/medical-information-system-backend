using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Tracing;
using WebApi.DtoModels.Enums;

namespace WebApi.DtoModels.Diagnosis;

public class DiagnosisCreateModel
{
    [Required]
    public Guid IcdDiagnosisId { get; set; }
    [MaxLength(5000)]
    public string? Description { get; set; } = string.Empty;
    [Required]
    public DiagnosisType Type { get; set; }
}