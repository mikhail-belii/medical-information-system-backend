using System.ComponentModel;
using BusinessLogic.ServiceInterfaces;
using Common;
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
    /// <param name="id">Inspection's identifier</param>
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
    public async Task<ActionResult<InspectionModel>> GetInspection([FromRoute(Name = "id")] Guid id)
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
    
    /// <summary>
    /// Edit concrete inspection
    /// </summary>
    /// <param name="id">Inspection's identifier</param>
    /// <response code="200">Success</response>
    /// <response code="400">Invalid arguments</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">User doesn't have editing rights (not the inspection author)</response>
    /// <response code="404">Inspection not found</response>
    /// <response code="500">InternalServerError</response>
    [Authorize]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = null!)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = null!)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = null!)]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = null!)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = null!)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
    public async Task<IActionResult> EditInspection([FromRoute(Name = "id")] Guid id, 
        [FromBody] InspectionEditModel model)
    {
        var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        var userId = await _tokenService.GetUserIdByToken(token);
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        try
        {
            await _inspectionService.EditInspection(id, model, userId);
            return Ok();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ResponseModel
            {
                Message = "Error",
                Status = $"Inspection with id '{id}' not found"
            });
        }
        catch (ForbiddenException ex)
        {
            return Problem(
                type: "/docs/errors/forbidden",
                title: "Forbidden action.",
                detail: $"{ex.Message}",
                statusCode: StatusCodes.Status403Forbidden,
                instance: HttpContext.Request.Path
            );
        }
        catch (IncorrectModelException ex)
        {
            return BadRequest(new ResponseModel
            {
                Status = "Error",
                Message = ex.Message
            });
        }
    }
    
    /// <summary>
    /// Get medical inspection chain for root inspection
    /// </summary>
    /// <param name="id">Root inspection's identifier</param>
    /// <response code="200">Success</response>
    /// <response code="400">Bad request</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Not found</response>
    /// <response code="500">InternalServerError</response>
    [Authorize]
    [HttpGet("{id:guid}/chain")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<InspectionPreviewModel>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = null!)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = null!)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = null!)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
    public async Task<ActionResult<List<InspectionPreviewModel>>> GetInspectionChain([FromRoute(Name = "id")] Guid id)
    {
        var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        var userId = await _tokenService.GetUserIdByToken(token);
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        try
        {
            var list = await _inspectionService.GetInspectionChain(id);
            return Ok(list);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ResponseModel
            {
                Message = "Error",
                Status = $"Inspection with id '{id}' not found"
            });
        }
        catch (IncorrectModelException ex)
        {
            return BadRequest(new ResponseModel
            {
                Status = "Error",
                Message = ex.Message
            });
        }
    }
}