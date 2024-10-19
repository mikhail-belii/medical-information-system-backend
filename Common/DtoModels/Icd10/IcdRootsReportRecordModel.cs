using Common.Enums;

namespace Common.DtoModels.Icd10;

public class IcdRootsReportRecordModel
{
    public string? PatientName { get; set; } = string.Empty;
    public DateTime PatientBirthdate { get; set; }
    public Gender Gender { get; set; }
    public Dictionary<string, int>? VisitsByRoot { get; set; }
}