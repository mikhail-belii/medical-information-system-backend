using System.ComponentModel.DataAnnotations;
using WebApi.DtoModels.Enums;

namespace WebApi.DtoModels.Diagnosis;

public class DiagnosisModel
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    public DateTime CreateTime { get; set; }
    [Required]
    [MinLength(1)]
    public string Code { get; set; } = string.Empty;
    [Required]
    [MinLength(1)]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    [Required]
    public DiagnosisType Type { get; set; }
}