using System.ComponentModel.DataAnnotations;
using WebApi.DtoModels.Enums;

namespace WebApi.DtoModels.Patient;

public class PatientModel
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    public DateTime CreateTime { get; set; }
    [Required]
    [MinLength(1)]
    public string Name { get; set; } = string.Empty;
    public DateTime? Birthday { get; set; }
    [Required]
    public Gender Gender { get; set; }
}