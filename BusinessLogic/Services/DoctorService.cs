using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BusinessLogic.ServiceInterfaces;
using Common.DbModels;
using Common.DtoModels.Doctor;
using Common.DtoModels.Others;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BusinessLogic.Services;

internal class DoctorService : IDoctorService
{
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _dbContext;
    
    public DoctorService(IConfiguration configuration, AppDbContext dbContext)
    {
        _configuration = configuration;
        _dbContext = dbContext;
    }
    
    public async Task<(TokenResponseModel, Guid)> Register(DoctorRegisterModel doctorRegisterModel)
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

        var token = CreateToken(doctorEntity);

        return (token, doctorEntity.Id);
    }

    public async Task<(TokenResponseModel, Guid)> Login(LoginCredentialsModel loginCredentialsModel)
    {
        var doctor = await _dbContext.Doctors
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Email.ToLower() == loginCredentialsModel.Email.ToLower() && d.Password == loginCredentialsModel.Password);

        if (doctor != null)
        {
            var token = CreateToken(doctor);
            return (token, doctor.Id);
        }
        return (new TokenResponseModel
        {
            Token = ""
        }, Guid.Empty);
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

    public TokenResponseModel CreateToken(DoctorEntity doctorEntity)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new Claim(ClaimTypes.Name, doctorEntity.Id.ToString()),
                new Claim(ClaimTypes.Email, doctorEntity.Email)
            ]),
            Expires = DateTime.UtcNow.AddMinutes(60),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"]
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenResponseModel = new TokenResponseModel
        {
            Token = tokenHandler.WriteToken(token)
        };

        return tokenResponseModel;
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