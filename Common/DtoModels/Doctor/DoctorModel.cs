using System.ComponentModel.DataAnnotations;
using Common.Enums;

namespace Common.DtoModels.Doctor;

public class DoctorModel
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
    [Required]
    [EmailAddress]
    [MinLength(1)]
    public string Email { get; set; } = string.Empty;
    [Phone]
    public string? Phone { get; set; } = string.Empty;
    [Required]
    public Guid Speciality { get; set; }
}