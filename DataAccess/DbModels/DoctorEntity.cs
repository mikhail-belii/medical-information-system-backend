using WebApi.DtoModels.Enums;

namespace DataAccess.DbModels;

public class DoctorEntity
{
    public Guid Id { get; set; }
    public DateTime CreateTime { get; set; }
    public string Name { get; set; }
    public DateTime Birthday { get; set; }
    public Gender Gender { get; set; }
    public SpecialityEntity Speciality { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
}