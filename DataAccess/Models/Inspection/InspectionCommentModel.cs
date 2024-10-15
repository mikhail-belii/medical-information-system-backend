using System.ComponentModel.DataAnnotations;
using DataAccess.Models.Doctor;

namespace DataAccess.Models.Inspection;

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