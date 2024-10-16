using System.ComponentModel.DataAnnotations;

namespace WebApi.DtoModels.Inspection;

public class InspectionCommentCreateModel
{
    [Required]
    [MinLength(1)]
    [MaxLength(1000)]
    public string Content { get; set; } = string.Empty;
}