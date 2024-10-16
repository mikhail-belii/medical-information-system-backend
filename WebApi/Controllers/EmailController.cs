using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;


[ApiController]
[Route("EmailSender")]
public class EmailController : ControllerBase
{
    private readonly IEmailSender _emailSender;

    public EmailController(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }
    [HttpGet]
    public async Task<IActionResult> SendEmail()
    {
        await _emailSender.SendEmailAsync("Mister pozitiv4ikmishanya", "sasama15555@gmail.com", "test", "privetiki");
        return Ok("200");
    }
}