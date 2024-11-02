using Common.DbModels;
using Common.DtoModels.Doctor;
using Common.DtoModels.Others;

namespace BusinessLogic.ServiceInterfaces;

public interface IDoctorService
{
    public Task<TokenResponseModel> Register(DoctorRegisterModel doctorRegisterModel);

    public Task<TokenResponseModel> Login(LoginCredentialsModel loginCredentialsModel);

    public Task<bool> IsEmailUnique(string email);

    public Task<bool> IsSpecialityExisting(DoctorRegisterModel doctorRegisterModel);
    
    public Task<DoctorEntity?> GetDoctorById(Guid id);

    public Task<DoctorModel> GetProfile(Guid userId);

    public Task EditProfile(Guid id, DoctorEditModel doctorEditModel);
}