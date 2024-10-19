namespace Common.DbModels;

public class Icd10Entity
{
    public Guid Id { get; set; }
    public Guid Icd10ParentId { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
}