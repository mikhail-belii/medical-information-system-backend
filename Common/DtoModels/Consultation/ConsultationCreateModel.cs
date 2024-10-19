using System.ComponentModel.DataAnnotations;
using Common.DtoModels.Inspection;

namespace Common.DtoModels.Consultation;

public class ConsultationCreateModel
{
    [Required]
    public Guid SpecialityId { get; set; }
    [Required]
    public InspectionCommentCreateModel Comment { get; set; }
}