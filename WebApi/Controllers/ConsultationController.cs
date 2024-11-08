using System.ComponentModel;
using BusinessLogic.ServiceInterfaces;
using Common;
using Common.DtoModels.Comment;
using Common.DtoModels.Consultation;
using Common.DtoModels.Inspection;
using Common.DtoModels.Others;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/consultation")]
public class ConsultationController : ControllerBase
{
    private readonly IConsultationService _consultationService;
    private readonly ITokenService _tokenService;

    public ConsultationController(IConsultationService consultationService, ITokenService tokenService)
    {
        _consultationService = consultationService;
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
    /// Get a list of medical inspections for consultation
    /// </summary>
    /// <param name="grouped">flag - whether grouping by inspection chain is required - for filtration</param>
    /// <param name="icdRoots">root elements for ICD-10 - for filtration</param>
    /// <param name="page">page number</param>
    /// <param name="size">required number of elements per page</param>
    /// <response code="200">Inspections for consultation list retrieved</response>
    /// <response code="400">Invalid arguments for filtration/pagination</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Not found</response>
    /// <response code="500">InternalServerError</response>
    [Authorize]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InspectionPagedListModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = null!)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = null!)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = null!)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
    public async Task<ActionResult<InspectionPagedListModel>> GetInspectionsList(
        [FromQuery(Name = "grouped"), DefaultValue(false)] bool? grouped,
        [FromQuery(Name = "icdRoots")] List<Guid>? icdRoots,
        [FromQuery(Name = "page"), DefaultValue(1)] int page,
        [FromQuery(Name = "size"), DefaultValue(5)] int size)
    {
        try
        {
            await EnsureTokenIsValid();

            if (size <= 0 || page <= 0)
            {
                return BadRequest(new ResponseModel
                {
                    Status = "Error",
                    Message = "Page and size values must be greater than 0"
                });
            }

            var list = await _consultationService.GetInspectionsList(
                grouped == true,
                icdRoots.Count == 0 ? new List<Guid>() : icdRoots,
                page,
                size);
            return Ok(list);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
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
    /// Get concrete consultation
    /// </summary>
    /// <param name="id">Consultation's identifier</param>
    /// <response code="200">Success</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Not found</response>
    /// <response code="500">InternalServerError</response>
    [Authorize]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ConsultationModel))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = null!)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = null!)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
    public async Task<ActionResult<ConsultationModel>> GetConsultation([FromRoute(Name = "id")] Guid id)
    {
        try
        {
            await EnsureTokenIsValid();

            var consultation = await _consultationService.GetConsultation(id);
            return Ok(consultation);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ResponseModel
            {
                Status = "Error",
                Message = $"Consultation with id '{id}' not found"
            });
        }
    }
    
    /// <summary>
    /// Add comment to concrete consultation
    /// </summary>
    /// <param name="id">Consultation's identifier</param>
    /// <response code="200">Success</response>
    /// <response code="400">Invalid arguments</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">User cannot add comment to consultation (unsuitable specialty and not the inspection author)</response>
    /// <response code="404">Consultation or parent comment not found</response>
    /// <response code="500">InternalServerError</response>
    [Authorize]
    [HttpPost("{id:guid}/comment")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Guid))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = null!)]   
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = null!)]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = null!)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = null!)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
    public async Task<ActionResult<Guid>> AddComment(
        [FromRoute(Name = "id")] Guid id,
        [FromBody] CommentCreateModel model)
    {
        try
        {
            var userId = await EnsureTokenIsValid();

            var commentId = await _consultationService.AddComment(id, model, userId);
            return Ok(commentId);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (KeyNotFoundException ex)
        {
            if (ex.Message == "Consultation")
            {
                return NotFound(new ResponseModel
                {
                    Status = "Error",
                    Message = $"Consultation with id '{id}' not found"
                });
            }

            return NotFound(new ResponseModel
            {
                Status = "Error",
                Message = $"Comment with id '{model.ParentId}' not found"
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
    }
    
        /// <summary>
    /// Edit comment
    /// </summary>
    /// <param name="id">Comment's identifier</param>
    /// <response code="200">Success</response>
    /// <response code="400">Invalid arguments</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">User is not the author of the comment</response>
    /// <response code="404">Comment not found</response>
    /// <response code="500">InternalServerError</response>
    [Authorize]
    [HttpPut("comment/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = null!)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = null!)]   
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = null!)]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = null!)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = null!)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
    public async Task<IActionResult> EditComment(
            [FromRoute(Name = "id")] Guid id,
            [FromBody] InspectionCommentCreateModel model)
    {
        try
        {
            var userId = await EnsureTokenIsValid();

            await _consultationService.EditComment(id, model, userId);
            return Ok();
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ResponseModel
            {
                Status = "Error",
                Message = $"Comment with id '{id}' not found"
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
    }
}