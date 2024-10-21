using Common.Enums;

namespace Common.DbModels;

public class DiagnosisEntity
{
    public Guid Id { get; set; }
    public DateTime CreateTime { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public DiagnosisType Type { get; set; }
    public Guid Icd10Id { get; set; }
}