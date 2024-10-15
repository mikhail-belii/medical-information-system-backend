using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Tracing;
using DataAccess.Models.Enums;

namespace DataAccess.Models.Diagnosis;

public class DiagnosisCreateModel
{
    [Required]
    public Guid IcdDiagnosisId { get; set; }
    [MaxLength(5000)]
    public string? Description { get; set; } = string.Empty;
    [Required]
    public DiagnosisType Type { get; set; }
}