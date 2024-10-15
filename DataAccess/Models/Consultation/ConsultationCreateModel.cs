using System.ComponentModel.DataAnnotations;
using DataAccess.Models.Inspection;

namespace DataAccess.Models.Consultation;

public class ConsultationCreateModel
{
    [Required]
    public Guid SpecialityId { get; set; }
    [Required]
    public InspectionCommentCreateModel Comment { get; set; }
}