using System.ComponentModel.DataAnnotations;
using WebApi.DtoModels.Diagnosis;

namespace WebApi.DtoModels.Inspection;

public class InspectionShortModel
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    public DateTime CreateTime { get; set; }
    [Required]
    public DateTime Date { get; set; }
    [Required]
    public DiagnosisModel Diagnosis { get; set; }
}