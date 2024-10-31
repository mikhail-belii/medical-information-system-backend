using System.ComponentModel;
using BusinessLogic.ServiceInterfaces;
using Common;
using Common.DtoModels.Inspection;
using Common.DtoModels.Others;
using Common.DtoModels.Patient;
using Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Security;

namespace WebApi.Controllers;

[ApiController]
[Route("api/patient")]
public class PatientController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly IPatientService _patientService;

    public PatientController(ITokenService tokenService, IPatientService patientService)
    {
        _tokenService = tokenService;
        _patientService = patientService;
    }

    /// <summary>
    /// Create new patient
    /// </summary>
    /// <response code="200">Patient was registered</response>
    /// <response code="400">Invalid arguments</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="500">InternalServerError</response>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Guid))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = null!)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = null!)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
    public async Task<ActionResult<Guid>> CreatePatient([FromBody] PatientCreateModel model)
    {
        var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        var userId = await _tokenService.GetUserIdByToken(token);
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var isValid = ModelState.IsValid;
        
        if (model.Birthday > DateTime.UtcNow)
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
        
        var patientId = await _patientService.CreatePatient(model, userId);
        return Ok(patientId);
    }

    /// <summary>
    /// Get patients list
    /// </summary>
    /// <param name="name">part of the name for filtering</param>
    /// <param name="conclusions">conclusion list to filter by conclusions</param>
    /// <param name="sorting">option to sort patients</param>
    /// <param name="scheduledVisits">show only scheduled visits</param>
    /// <param name="onlyMine">show inspections done by this doctor</param>
    /// <param name="page">page number</param>
    /// <param name="size">required number of elements per page</param>
    /// <response code="200">Patients paged list retrieved</response>
    /// <response code="400">Invalid arguments for filtration/pagination/sorting</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="500">InternalServerError</response>
    [Authorize]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PatientPagedListModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = null!)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = null!)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
    public async Task<ActionResult<PatientPagedListModel>> GetPatientsList(
        [FromQuery(Name = "name")] string? name,
        [FromQuery(Name = "conclusions")] List<Conclusion>? conclusions,
        [FromQuery(Name = "sorting")] PatientSorting? sorting,
        [FromQuery(Name = "scheduledVisits"), DefaultValue(false)] bool? scheduledVisits,
        [FromQuery(Name = "onlyMine"), DefaultValue(false)] bool? onlyMine,
        [FromQuery(Name = "page"), DefaultValue(1)] int page,
        [FromQuery(Name = "size"), DefaultValue(5)] int size
        )
    {
        var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        var userId = await _tokenService.GetUserIdByToken(token);
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        if (size <= 0 || page <= 0)
        {
            return BadRequest(new ResponseModel
            {
                Status = "Error",
                Message = "Page and size values must be greater than 0"
            });
        }

        var list = await _patientService.GetPatientsList(
            (string.IsNullOrEmpty(name)) ? "" : name,
            conclusions,
            sorting,
            scheduledVisits == true,
            onlyMine == true,
            page,
            size,
            userId);
        return Ok(list);
    }
    
    /// <summary>
    /// Create inspection for specified patient
    /// </summary>
    /// <param name="id">Patient's identifier</param>
    /// <response code="200">Success</response>
    /// <response code="400">Bad request</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="500">InternalServerError</response>
    [Authorize]
    [HttpPost("{id:guid}/inspections")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Guid))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = null!)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = null!)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
    public async Task<ActionResult<Guid>> CreateInspection(
        [FromRoute(Name = "id")] Guid id,
        [FromBody] InspectionCreateModel model)
    {
        var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        var userId = await _tokenService.GetUserIdByToken(token);
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        try
        {
            var inspectionId = await _patientService.CreateInspection(model, userId, id);
            return Ok(inspectionId);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ResponseModel
            {
                Message = "Error",
                Status = $"Patient with id '{id}' not found"
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
    
    /// <summary>
    /// Get a list of patient medical inspections
    /// </summary>
    /// <param name="id">Patient's identifier</param>
    /// <param name="grouped">flag - whether grouping by inspection chain is required - for filtration</param>
    /// <param name="icdRoots">root elements for ICD-10 - for filtration</param>
    /// <param name="page">page number</param>
    /// <param name="size">required number of elements per page</param>
    /// <response code="200">Patient's inspections list retrieved</response>
    /// <response code="400">Invalid arguments for filtration/pagination</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Patient not found</response>
    /// <response code="500">InternalServerError</response>
    [Authorize]
    [HttpGet("{id:guid}/inspections")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InspectionPagedListModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = null!)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = null!)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = null!)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
    public async Task<ActionResult<InspectionPagedListModel>> GetInspectionsList(
        [FromRoute(Name = "id")] Guid id,
        [FromQuery(Name = "grouped"), DefaultValue(false)] bool? grouped,
        [FromQuery(Name = "icdRoots")] List<Guid>? icdRoots,
        [FromQuery(Name = "page"), DefaultValue(1)] int page,
        [FromQuery(Name = "size"), DefaultValue(5)] int size)
    {
        var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        var userId = await _tokenService.GetUserIdByToken(token);
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        if (size <= 0 || page <= 0)
        {
            return BadRequest(new ResponseModel
            {
                Status = "Error",
                Message = "Page and size values must be greater than 0"
            });
        }

        try
        {
            var list = await _patientService.GetInspectionsList(
                id,
                grouped == true,
                icdRoots.Count == 0 ? new List<Guid>() : icdRoots,
                page,
                size);
            return Ok(list);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ResponseModel
            {
                Message = "Error",
                Status = $"Patient with id '{id}' not found"
            });
        }

    }
    
    /// <summary>
    /// Get patient card
    /// </summary>
    /// <param name="id">Patient's identifier</param>
    /// <response code="200">Success</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Not Found</response>
    /// <response code="500">InternalServerError</response>
    [Authorize]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PatientModel))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = null!)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = null!)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
    public async Task<ActionResult<PatientModel>> GetPatient([FromRoute(Name = "id")] Guid id)
    {
        var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        var userId = await _tokenService.GetUserIdByToken(token);
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        try
        {
            return Ok(await _patientService.GetPatientById(id));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ResponseModel
            {
                Message = "Error",
                Status = $"Patient with id '{id}' not found"
            });
        }
    }
    
    /// <summary>
    /// Search for patient medical inspections without child inspections
    /// </summary>
    /// <param name="id">Patient's identifier</param>
    /// <param name="request">part of the diagnosis name or code</param>
    /// <response code="200">Patient's inspections list retrieved</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Patient not found</response>
    /// <response code="500">InternalServerError</response>
    [Authorize]
    [HttpGet("{id:guid}/inspections/search")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<InspectionShortModel>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = null!)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = null!)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = null!)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
    public async Task<ActionResult<List<InspectionShortModel>>> GetInspectionsWithoutChildren(
        [FromRoute(Name = "id")] Guid id,
        [FromQuery(Name = "request")] string? request)
    {
        var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        var userId = await _tokenService.GetUserIdByToken(token);
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        try
        {
            return Ok(await _patientService.GetInspectionsWithoutChildren(
                id,
                string.IsNullOrEmpty(request) ? "" : request));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ResponseModel
            {
                Message = "Error",
                Status = $"Patient with id '{id}' not found"
            });
        }
    }
}