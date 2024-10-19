using System.ComponentModel.DataAnnotations;
using Common.Enums;

namespace Common.DtoModels.Patient;

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