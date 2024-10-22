using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using BusinessLogic.ServiceInterfaces;
using Common.DtoModels.Inspection;
using Common.DtoModels.Others;
using Common.DtoModels.Patient;
using Common.Enums;
using DataAccess.RepositoryInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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
        
        var patient = await _patientService.GetPatientById(id);
        if (patient is null)
        {
            return NotFound();
        }

        return Ok(patient);
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

        var inspectionId = await _patientService.CreateInspection(model, userId, id);
        return Ok(inspectionId);
    }
    
}