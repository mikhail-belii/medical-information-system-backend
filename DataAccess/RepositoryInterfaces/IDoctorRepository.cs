using Common.DbModels;
using Common.DtoModels.Doctor;
using Common.DtoModels.Others;

namespace DataAccess.RepositoryInterfaces;

public interface IDoctorRepository
{
    Task Register(DoctorEntity doctorEntity, CancellationToken cancellationToken = default);
    Task<DoctorEntity?> GetByCredentials(string email, string password);
    Task<bool> IsEmailUnique(string email);
    Task<bool> IsSpecialityExisting(Guid specialityId);
    Task<DoctorEntity?> GetDoctorById(Guid id);
    Task<DoctorEntity?> GetProfile(Guid id);
    Task EditProfile(Guid id, DoctorEditModel doctorEditModel);
    // void Read();
}