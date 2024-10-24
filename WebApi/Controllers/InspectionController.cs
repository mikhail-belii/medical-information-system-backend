using System.ComponentModel;
using BusinessLogic.ServiceInterfaces;
using Common.DtoModels.Inspection;
using Common.DtoModels.Others;
using DataAccess.RepositoryInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/inspection")]
public class InspectionController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly IInspectionService _inspectionService;

    public InspectionController(ITokenService tokenService, IInspectionService inspectionService)
    {
        _tokenService = tokenService;
        _inspectionService = inspectionService;
    }
    
    /// <summary>
    /// Get full information about specified inspection
    /// </summary>
    /// <response code="200">Inspection found and successfully extracted</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Not found</response>
    /// <response code="500">InternalServerError</response>
    [Authorize]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InspectionModel))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = null!)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = null!)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
    public async Task<ActionResult<InspectionModel>> GetInspection([FromRoute] Guid id)
    {
        var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        var userId = await _tokenService.GetUserIdByToken(token);
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        try
        {
            var inspection = await _inspectionService.GetInspection(id);
            return Ok(inspection);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ResponseModel
            {
                Message = "Error",
                Status = $"Inspection with id '{id}' not found"
            });
        }
    }
}