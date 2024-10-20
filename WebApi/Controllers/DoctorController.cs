using System.Text.RegularExpressions;
using BusinessLogic.ServiceInterfaces;
using Common;
using Common.DtoModels.Doctor;
using Common.DtoModels.Others;
using Common.Enums;
using DataAccess.RepositoryInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("doctor")]
public class DoctorController : ControllerBase
{
    private readonly IDoctorService _doctorService;
    private readonly ITokenService _tokenService;

    public DoctorController(IDoctorService doctorService, ITokenService tokenService)
    {
        _doctorService = doctorService;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<TokenResponseModel>> Register([FromBody] DoctorRegisterModel model)
    {
        bool isValid = ModelState.IsValid;

        if (!string.IsNullOrEmpty(model.Phone))
        {
            if (!Regex.IsMatch(model.Phone, RegexPatterns.Phone))
            {
                ModelState.AddModelError("Phone", 
                    "Invalid phone number format. It should start with +7 and then contain exactly 10 digits");
                isValid = false;
            }
        }
        
        if (!Regex.IsMatch(model.Email, RegexPatterns.Email))
        {
            ModelState.AddModelError("Email", 
                "Invalid email format");
            isValid = false;
        }
        
        if (!await _doctorService.IsEmailUnique(model.Email))
        {
            ModelState.AddModelError("Email",
                "This email is already taken");
            isValid = false;
        }
        
        if (!Regex.IsMatch(model.Password, RegexPatterns.Password))
        {
            ModelState.AddModelError("Password", 
                "Password requires at least one digit");
            isValid = false;
        }

        if (model.Gender is not (Gender.Female or Gender.Male))
        {
            ModelState.AddModelError("Gender",
                "Gender must be Male or Female");
            isValid = false;
        }

        if (model.Birthday > DateTime.UtcNow)
        {
            ModelState.AddModelError("Birthday",
                "Birth date can't be later than today");
            isValid = false;
        }

        if (!await _doctorService.IsSpecialityExisting(model))
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel
            {
                Status = "Error",
                Message = "Speciality"
            });
        }

        if (!isValid)
        {
            return BadRequest(ModelState);
        }

        var (token, id) = await _doctorService.Register(model);
        await _tokenService.AddToken(id, token.Token);
        return Ok(token);
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenResponseModel>> Login([FromBody] LoginCredentialsModel model)
    {
        var (token, id) = await _doctorService.Login(model);
        if (token.Token.Equals(""))
        {
            return BadRequest(new ResponseModel
            {
                Status = "Error",
                Message = "Login failed"
            });
        }

        await _tokenService.AddToken(id, token.Token);
        return Ok(token);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        await _tokenService.RemoveToken(token);
        return Ok(new ResponseModel
        {
            Status = null,
            Message = "Logged out"
        });
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<ActionResult<DoctorModel>> GetProfile()
    {
        var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        var userId = await _tokenService.GetUserIdByToken(token);
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }
        var doctorModel = await _doctorService.GetProfile(userId);
        return Ok(doctorModel);
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> ChangeProfile([FromBody] DoctorEditModel model)
    {
        var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        var userId = await _tokenService.GetUserIdByToken(token);
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }
        
        bool isValid = ModelState.IsValid;
        
        if (!string.IsNullOrEmpty(model.Phone))
        {
            if (!Regex.IsMatch(model.Phone, RegexPatterns.Phone))
            {
                ModelState.AddModelError("Phone", 
                    "Invalid phone number format. It should start with +7 and then contain exactly 10 digits");
                isValid = false;
            }
        }
        
        if (!Regex.IsMatch(model.Email, RegexPatterns.Email))
        {
            ModelState.AddModelError("Email", 
                "Invalid email format");
            isValid = false;
        }

        if (!await _doctorService.IsEmailUnique(model.Email))
        {
            var doctor = await _doctorService.GetDoctorById(userId);
            if (doctor.Email != model.Email)
            {
                ModelState.AddModelError("Email",
                    $"Email '{model.Email}' is already taken");
                isValid = false;
            }
        }
        
        if (model.BirthDay > DateTime.UtcNow)
        {
            ModelState.AddModelError("Birthday",
                "Birth date can't be later than today");
            isValid = false;
        }
        
        if (model.Gender is not (Gender.Female or Gender.Male))
        {
            ModelState.AddModelError("Gender",
                "Gender must be Male or Female");
            isValid = false;
        }
        
        if (!isValid)
        {
            return BadRequest(ModelState);
        }

        await _doctorService.EditProfile(userId, model);
        return Ok();
    }
}