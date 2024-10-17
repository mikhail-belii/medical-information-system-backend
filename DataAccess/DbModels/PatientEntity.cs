using WebApi.DtoModels.Enums;

namespace DataAccess.DbModels;

public class PatientEntity
{
    public Guid Id { get; set; }
    public DateTime CreateTime { get; set; }
    public string Name { get; set; }
    public DateTime Birthday { get; set; }
    public Gender Gender { get; set; }
}