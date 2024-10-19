using Common.DbModels;
using Common.DtoModels.Others;

namespace DataAccess.RepositoryInterfaces;

public interface IDoctorRepository
{
    Task Register(DoctorEntity doctorEntity, CancellationToken cancellationToken = default);
    Task<DoctorEntity?> GetByCredentials(string email, string password);
    Task<bool> IsEmailUnique(string email);
}