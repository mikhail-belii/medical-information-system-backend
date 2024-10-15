using System.ComponentModel.DataAnnotations;
using DataAccess.Models.Enums;

namespace DataAccess.Models.Doctor;

public class DoctorRegisterModel
{
    [Required]
    [MinLength(1)]
    [MaxLength(1000)]
    public string Name { get; set; } = string.Empty;
    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
    [Required]
    [EmailAddress]
    [MinLength(1)]
    public string Email { get; set; } = string.Empty;
    public DateTime? Birthday { get; set; }
    [Required]
    public Gender Gender { get; set; }
    [Phone]
    public string? Phone { get; set; } = string.Empty;
    [Required]
    public Guid Speciality { get; set; }
}