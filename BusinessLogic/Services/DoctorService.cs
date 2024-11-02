using BusinessLogic.ServiceInterfaces;
using Common.DbModels;
using Common.DtoModels.Doctor;
using Common.DtoModels.Others;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Services;

internal class DoctorService : IDoctorService
{
    private readonly AppDbContext _dbContext;
    private readonly ITokenService _tokenService;
    
    public DoctorService(AppDbContext dbContext, ITokenService tokenService)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
    }
    
    public async Task<TokenResponseModel> Register(DoctorRegisterModel doctorRegisterModel)
    {
        DoctorEntity doctorEntity = new DoctorEntity()
        {
            Id = new Guid(),
            CreateTime = DateTime.UtcNow,
            Name = doctorRegisterModel.Name,
            Birthday = doctorRegisterModel.Birthday,
            Gender = doctorRegisterModel.Gender,
            Speciality = doctorRegisterModel.Speciality,
            Email = doctorRegisterModel.Email,
            Password = doctorRegisterModel.Password,
            Phone = doctorRegisterModel.Phone,
            Inspections = new List<InspectionEntity>(),
            Comments = new List<CommentEntity>()
        };

        await _dbContext.Doctors.AddAsync(doctorEntity);
        await _dbContext.SaveChangesAsync();

        var strToken = await _tokenService.CreateToken(doctorEntity.Id);
        var token = new TokenResponseModel
        {
            Token = strToken
        };

        return token;
    }

    public async Task<TokenResponseModel> Login(LoginCredentialsModel loginCredentialsModel)
    {
        var doctor = await _dbContext.Doctors
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Email.ToLower() == loginCredentialsModel.Email.ToLower() && d.Password == loginCredentialsModel.Password);

        if (doctor != null)
        {
            var strToken = await _tokenService.CreateToken(doctor.Id);
            var token = new TokenResponseModel
            {
                Token = strToken
            };
            return token;
        }
        return (new TokenResponseModel { Token = "" });
    }

    public async Task<bool> IsEmailUnique(string email)
    {
        var doctor = await _dbContext.Doctors
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email == email);
        return doctor == null;
    }

    public async Task<bool> IsSpecialityExisting(DoctorRegisterModel doctorRegisterModel)
    {
        var speciality = await _dbContext.Specialities
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == doctorRegisterModel.Speciality);
        return speciality != null;
    }

    public async Task<DoctorEntity?> GetDoctorById(Guid id)
    {
        return await _dbContext.Doctors.FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<DoctorModel> GetProfile(Guid userId)
    {
        var doctorEntity = await _dbContext.Doctors
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == userId);
        
        var doctorModel = new DoctorModel
        {
            Id = doctorEntity.Id,
            CreateTime = doctorEntity.CreateTime,
            Name = doctorEntity.Name,
            Birthday = doctorEntity.Birthday,
            Gender = doctorEntity.Gender,
            Email = doctorEntity.Email,
            Phone = doctorEntity.Phone,
            Speciality = doctorEntity.Speciality
        };
        return doctorModel;
    }

    public async Task EditProfile(Guid id, DoctorEditModel doctorEditModel)
    {
        await _dbContext.Doctors
            .Where(d => d.Id == id)
            .ExecuteUpdateAsync(x => x
                .SetProperty(e => e.Email, doctorEditModel.Email)
                .SetProperty(e => e.Name, doctorEditModel.Name)
                .SetProperty(e => e.Birthday, doctorEditModel.BirthDay)
                .SetProperty(e => e.Gender, doctorEditModel.Gender)
                .SetProperty(e => e.Phone, doctorEditModel.Phone));
        await _dbContext.SaveChangesAsync();
    }
}