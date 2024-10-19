using System.Text.RegularExpressions;
using BusinessLogic.ServiceInterfaces;
using Common;
using Common.DtoModels.Doctor;
using Common.DtoModels.Others;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("doctor")]
public class DoctorController : ControllerBase
{
    private readonly IDoctorService _doctorService;

    public DoctorController(IDoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] DoctorRegisterModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!string.IsNullOrEmpty(model.Phone))
        {
            if (!Regex.IsMatch(model.Phone, RegexPatterns.Phone))
            {
                ModelState.AddModelError("Phone", 
                    "Invalid phone number format. It should start with +7 and then contain exactly 10 digits.");
                return BadRequest(ModelState);
            }
        }

        if (!Regex.IsMatch(model.Email, RegexPatterns.Email))
        {
            ModelState.AddModelError("Email", 
                "Invalid email format.");
            return BadRequest(ModelState);
        }

        if (!await _doctorService.IsEmailUnique(model))
        {
            return BadRequest(new ResponseModel
            {
                Status = "Error",
                Message = "This email is already taken"
            });
        }

        var token = await _doctorService.Register(model);
        return Ok(token);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCredentialsModel model)
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
}