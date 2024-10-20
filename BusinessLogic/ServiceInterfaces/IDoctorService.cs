using Common.DbModels;
using Common.DtoModels.Doctor;
using Common.DtoModels.Others;

namespace BusinessLogic.ServiceInterfaces;

public interface IDoctorService
{
    public Task<(TokenResponseModel, Guid)> Register(DoctorRegisterModel doctorRegisterModel,
        CancellationToken cancellationToken = default);

    public Task<(TokenResponseModel, Guid)> Login(LoginCredentialsModel loginCredentialsModel,
        CancellationToken cancellationToken = default);

    public Task<bool> IsEmailUnique(string email,
        CancellationToken cancellationToken = default);

    public Task<bool> IsSpecialityExisting(DoctorRegisterModel doctorRegisterModel,
        CancellationToken cancellationToken = default);
    
    public Task<DoctorEntity?> GetDoctorById(Guid id);

    public TokenResponseModel CreateToken(DoctorEntity doctorEntity);

    public Task<DoctorModel> GetProfile(Guid userId);

    public Task EditProfile(Guid id, DoctorEditModel doctorEditModel);
}