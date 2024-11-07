using System.Text.RegularExpressions;
using BusinessLogic.ServiceInterfaces;
using Common;
using Common.DtoModels.Doctor;
using Common.DtoModels.Others;
using Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/doctor")]
public class DoctorController : ControllerBase
{
    private readonly IDoctorService _doctorService;
    private readonly ITokenService _tokenService;

    public DoctorController(IDoctorService doctorService, ITokenService tokenService)
    {
        _doctorService = doctorService;
        _tokenService = tokenService;
    }

    private async Task<Guid> EnsureTokenIsValid()
    {
        var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        if (! await _tokenService.IsTokenValid(token))
        {
            throw new UnauthorizedAccessException();
        }
        
        return await _tokenService.GetUserIdFromToken(token);
    }
    
    /// <summary>
    /// Register new user
    /// </summary>
    /// <response code="200">Doctor was registered</response>
    /// <response code="400">Invalid arguments</response>
    /// <response code="500">InternalServerError</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TokenResponseModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = null!)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<TokenResponseModel>> Register([FromBody] DoctorRegisterModel model)
    {
        var isValid = ModelState.IsValid;

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

        var token = await _doctorService.Register(model);
        return Ok(token);
    }
    
    
    /// <summary>
    /// Log in to the system
    /// </summary>
    /// <response code="200">Doctor was registered</response>
    /// <response code="400">Invalid arguments</response>
    /// <response code="500">InternalServerError</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TokenResponseModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = null!)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<TokenResponseModel>> Login([FromBody] LoginCredentialsModel model)
    {
        var token = await _doctorService.Login(model);
        if (token.Token.Equals(""))
        {
            return BadRequest(new ResponseModel
            {
                Status = "Error",
                Message = "Login failed"
            });
        }
        return Ok(token);
    }

    
    /// <summary>
    /// Log out system user
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="500">InternalServerError</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = null!)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var userId = await EnsureTokenIsValid();
            var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            await _tokenService.AddInvalidToken(token);
            return Ok(new ResponseModel
            {
                Status = null,
                Message = "Logged out"
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
    }

    
    /// <summary>
    /// Get user profile
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Not found</response>
    /// <response code="500">InternalServerError</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DoctorModel))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = null!)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = null!)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
    [Authorize]
    [HttpGet("profile")]
    public async Task<ActionResult<DoctorModel>> GetProfile()
    {
        try
        {
            var userId = await EnsureTokenIsValid();
            var doctorModel = await _doctorService.GetProfile(userId);

            return Ok(doctorModel);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Edit user profile
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="400">Bad request</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Not found</response>
    /// <response code="500">InternalServerError</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = null!)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = null!)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = null!)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = null!)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> ChangeProfile([FromBody] DoctorEditModel model)
    {
        try
        {
            var userId = await EnsureTokenIsValid();

            var isValid = ModelState.IsValid;

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
                if (doctor?.Email != model.Email)
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
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception)
        {
            return NotFound();
        }
    }
}