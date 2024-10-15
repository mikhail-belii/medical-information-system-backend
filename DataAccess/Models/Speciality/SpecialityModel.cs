using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models.Speciality;

public class SpecialityModel
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    public DateTime CreateTime { get; set; }
    [Required]
    [MinLength(1)]
    public string Name { get; set; } = string.Empty;
}