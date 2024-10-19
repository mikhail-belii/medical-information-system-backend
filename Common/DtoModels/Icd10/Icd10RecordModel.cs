using System.ComponentModel.DataAnnotations;

namespace Common.DtoModels.Icd10;

public class Icd10RecordModel
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    public DateTime CreateTime { get; set; }
    public string? Code { get; set; } = string.Empty;
    public string? Name { get; set; } = string.Empty;
}