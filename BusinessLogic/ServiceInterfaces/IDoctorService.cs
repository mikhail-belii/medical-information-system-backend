using Common.DbModels;
using Common.DtoModels.Doctor;
using Common.DtoModels.Others;

namespace BusinessLogic.ServiceInterfaces;

public interface IDoctorService
{
    public Task<TokenResponseModel> Register(DoctorRegisterModel doctorRegisterModel,
        CancellationToken cancellationToken = default);

    public Task<TokenResponseModel> Login(LoginCredentialsModel loginCredentialsModel,
        CancellationToken cancellationToken = default);

    public Task<bool> IsEmailUnique(DoctorRegisterModel doctorRegisterModel,
        CancellationToken cancellationToken = default);

    public TokenResponseModel CreateToken(DoctorEntity doctorEntity);
}