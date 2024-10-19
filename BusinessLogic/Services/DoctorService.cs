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
    
    public async Task<TokenResponseModel> Register(DoctorRegisterModel doctorRegisterModel,
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

        return token;
    }

    public async Task<TokenResponseModel> Login(LoginCredentialsModel loginCredentialsModel, CancellationToken cancellationToken = default)
    {
        var doctor = await _doctorRepository
            .GetByCredentials(loginCredentialsModel.Email, loginCredentialsModel.Password);

        if (doctor != null)
        {
            var token = CreateToken(doctor);
            return token;
        }
        else
        {
            return new TokenResponseModel
            {
                Token = ""
            };
        }
    }

    public async Task<bool> IsEmailUnique(DoctorRegisterModel doctorRegisterModel,
        CancellationToken cancellationToken = default)
    {
        return await _doctorRepository.IsEmailUnique(doctorRegisterModel.Email);
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
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenResponseModel = new TokenResponseModel
        {
            Token = tokenHandler.WriteToken(token)
        };

        return tokenResponseModel;
    }
}