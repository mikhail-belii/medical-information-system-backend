using System.ComponentModel.DataAnnotations;
using Common.Enums;

namespace Common.DtoModels.Doctor;

public class DoctorEditModel
{
    [Required]
    [EmailAddress]
    [MinLength(1)]
    public string Email { get; set; } = string.Empty;
    [Required]
    [MinLength(1)]
    [MaxLength(1000)]
    public string Name { get; set; } = string.Empty;
    public DateTime? BirthDay { get; set; }
    [Required]
    public Gender Gender { get; set; }
    [Phone]
    public string? Phone { get; set; } = string.Empty;
}