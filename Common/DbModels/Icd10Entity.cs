namespace Common.DbModels;

public class Icd10Entity
{
    public Guid Id { get; set; }
    public string IcdId { get; set; }
    public string? IcdParentId { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
}