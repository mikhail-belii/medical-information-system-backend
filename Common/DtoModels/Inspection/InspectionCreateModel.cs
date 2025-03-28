﻿using System.ComponentModel.DataAnnotations;
using Common.DtoModels.Consultation;
using Common.DtoModels.Diagnosis;
using Common.Enums;

namespace Common.DtoModels.Inspection;

public class InspectionCreateModel
{
    [Required]
    public DateTime Date { get; set; }
    [Required]
    [MinLength(1)]
    [MaxLength(5000)]
    public string Anamnesis { get; set; } = string.Empty;
    [Required]
    [MinLength(1)]
    [MaxLength(5000)]
    public string Complaints { get; set; } = string.Empty;
    [Required]
    [MinLength(1)]
    [MaxLength(5000)]
    public string Treatment { get; set; } = string.Empty;
    [Required]
    public Conclusion Conclusion { get; set; }
    public DateTime? NextVisitDate { get; set; }
    public DateTime? DeathDate { get; set; }
    public Guid? PreviousInspectionId { get; set; }
    [Required]
    [MinLength(1)]
    public List<DiagnosisCreateModel> Diagnoses { get; set; }
    public List<ConsultationCreateModel>? Consultations { get; set; }
}