using Common.Enums;

namespace Common.DbModels;

public class DoctorEntity
{
    public Guid Id { get; set; }
    public DateTime CreateTime { get; set; }
    public string Name { get; set; }
    public DateTime? Birthday { get; set; }
    public Gender Gender { get; set; }
    public Guid Speciality { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string? Phone { get; set; }
    public List<InspectionEntity> Inspections { get; set; }
    public List<CommentEntity> Comments { get; set; }
}