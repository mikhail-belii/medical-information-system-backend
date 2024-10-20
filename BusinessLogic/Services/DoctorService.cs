using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BusinessLogic.ServiceInterfaces;
using Common.DbModels;
using DataAccess.RepositoryInterfaces;
using Common.DtoModels.Doctor;
using Common.DtoModels.Others;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BusinessLogic.Services;

internal class DoctorService : IDoctorService
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly IConfiguration _configuration;
    
    public DoctorService(IDoctorRepository doctorRepository, IConfiguration configuration)
    {
        _doctorRepository = doctorRepository;
        _configuration = configuration;
    }
    
    public async Task<(TokenResponseModel, Guid)> Register(DoctorRegisterModel doctorRegisterModel,
        CancellationToken cancellationToken = default)
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

        await _doctorRepository.Register(doctorEntity, cancellationToken: default);

        var token = CreateToken(doctorEntity);

        return (token, doctorEntity.Id);
    }

    public async Task<(TokenResponseModel, Guid)> Login(LoginCredentialsModel loginCredentialsModel, CancellationToken cancellationToken = default)
    {
        var doctor = await _doctorRepository
            .GetByCredentials(loginCredentialsModel.Email, loginCredentialsModel.Password);

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

    public async Task<bool> IsEmailUnique(string email,
        CancellationToken cancellationToken = default)
    {
        return await _doctorRepository.IsEmailUnique(email);
    }

    public async Task<bool> IsSpecialityExisting(DoctorRegisterModel doctorRegisterModel,
        CancellationToken cancellationToken = default)
    {
        return await _doctorRepository.IsSpecialityExisting(doctorRegisterModel.Speciality);
    }

    public async Task<DoctorEntity?> GetDoctorById(Guid id)
    {
        return await _doctorRepository.GetDoctorById(id);
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
        var doctorEntity = await _doctorRepository.GetProfile(userId);
        
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
        await _doctorRepository.EditProfile(id, doctorEditModel);
    }
}