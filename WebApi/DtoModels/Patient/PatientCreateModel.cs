using System.ComponentModel.DataAnnotations;
using WebApi.DtoModels.Enums;

namespace WebApi.DtoModels.Patient;

public class PatientCreateModel
{
    [Required]
    [MinLength(1)]
    [MaxLength(1000)]
    public string Name { get; set; } = string.Empty;
    public DateTime? Birthday { get; set; }
    [Required]
    public Gender Gender { get; set; }
}