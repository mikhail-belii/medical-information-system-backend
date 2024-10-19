using System.ComponentModel.DataAnnotations;
using Common.DtoModels.Doctor;

namespace Common.DtoModels.Inspection;

public class InspectionCommentModel
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    public DateTime CreateTime { get; set; }
    public Guid? ParentId { get; set; }
    public string? Content { get; set; } = string.Empty;
    public DoctorModel? Author { get; set; }
    public DateTime? ModifyTime { get; set; }
}