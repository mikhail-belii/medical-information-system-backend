using Common.Enums;

namespace Common.DbModels;

public class PatientEntity
{
    public Guid Id { get; set; }
    public Guid DoctorId { get; set; }
    public DateTime CreateTime { get; set; }
    public string Name { get; set; }
    public DateTime? Birthday { get; set; }
    public Gender Gender { get; set; }
}