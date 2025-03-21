using System.ComponentModel.DataAnnotations;

namespace Common.DtoModels.Speciality;

public class CreateSpecialityModel
{
    [Required]
    public string Name { get; set; } = string.Empty;
}