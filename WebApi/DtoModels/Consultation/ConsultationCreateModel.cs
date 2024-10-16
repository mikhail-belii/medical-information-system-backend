using System.ComponentModel.DataAnnotations;
using WebApi.DtoModels.Inspection;

namespace WebApi.DtoModels.Consultation;

public class ConsultationCreateModel
{
    [Required]
    public Guid SpecialityId { get; set; }
    [Required]
    public InspectionCommentCreateModel Comment { get; set; }
}