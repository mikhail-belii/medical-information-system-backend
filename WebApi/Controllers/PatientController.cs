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
    /// Get patient card
    /// </summary>
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
    public async Task<ActionResult<PatientModel>> GetPatient([FromRoute] Guid id)
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
        catch (InvalidParameterException)
        {
            return NotFound(new ResponseModel
            {
                Message = "Error",
                Status = $"Patient with id '{id}' not found"
            });
        }
    }

    /// <summary>
    /// Get patients list
    /// </summary>
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
        [FromQuery] string? name,
        [FromQuery] List<Conclusion>? conclusions,
        [FromQuery] PatientSorting? sorting,
        [FromQuery, DefaultValue(false)] bool? scheduledVisits,
        [FromQuery, DefaultValue(false)] bool? onlyMine,
        [FromQuery, DefaultValue(1)] int page,
        [FromQuery, DefaultValue(5)] int size
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
        [FromRoute] Guid id,
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
        [FromRoute] Guid id,
        [FromQuery, DefaultValue(false)] bool? grouped,
        [FromQuery] List<Guid>? icdRoots,
        [FromQuery, DefaultValue(1)] int page,
        [FromQuery, DefaultValue(5)] int size
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
    /// Search for patient medical inspections without child inspections
    /// </summary>
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
        [FromRoute] Guid id,
        [FromQuery] string? request)
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