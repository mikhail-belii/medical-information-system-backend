using System.ComponentModel.DataAnnotations;
using BusinessLogic.ServiceInterfaces;
using Common.DtoModels.Icd10;
using Common.DtoModels.Others;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/report")]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly ITokenService _tokenService;

    public ReportController(IReportService reportService, ITokenService tokenService)
    {
        _reportService = reportService;
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
    /// Get a report on patients' visits based on ICD-10 roots for a specified time interval
    /// </summary>
    /// <param name="start">Start of time interval</param>
    /// <param name="end">End of time interval</param>
    /// <param name="icdRoots">Set of ICD-10 roots. All possible roots if null</param>
    /// <response code="200">Report extracted successfully</response>
    /// <response code="400">Some fields in request are invalid</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Access to report is forbidden for user</response>
    /// <response code="500">InternalServerError</response>
    [Authorize]
    [HttpGet("icdrootsreport")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IcdRootsReportModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = null!)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = null!)]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = null!)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
    public async Task<ActionResult<IcdRootsReportModel>> GetReport(
        [FromQuery(Name = "start")] [Required] DateTime start,
        [FromQuery(Name = "end")] [Required] DateTime end,
        [FromQuery(Name = "icdRoots")] List<Guid>? icdRoots)
    {
        try
        {
            await EnsureTokenIsValid();

            if (start >= end)
            {
                return BadRequest(new ResponseModel
                {
                    Status = "Error",
                    Message = "Start should be less than end"
                });
            }

            var model = await _reportService.GetReport(
                start,
                end,
                icdRoots == null ? new List<Guid>() : icdRoots);

            return Ok(model);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (KeyNotFoundException)
        {
            return BadRequest(new ResponseModel
            {
                Status = "Error",
                Message = "Incorrect id for ICD root"
            });
        }
    }
}