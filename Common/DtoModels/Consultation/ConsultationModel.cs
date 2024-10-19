using System.ComponentModel.DataAnnotations;
using Common.DtoModels.Comment;
using Common.DtoModels.Speciality;

namespace Common.DtoModels.Consultation;

public class ConsultationModel
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    public DateTime CreateTime { get; set; }
    public Guid InspectionId { get; set; }
    public SpecialityModel Speciality { get; set; }
    public List<CommentModel>? Comments { get; set; }
}