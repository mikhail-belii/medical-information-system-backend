using System.ComponentModel;
using BusinessLogic.ServiceInterfaces;
using Common.DtoModels.Icd10;
using Common.DtoModels.Others;
using Common.DtoModels.Patient;
using Common.DtoModels.Speciality;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/dictionary")]
public class DictionaryController : ControllerBase
{
    private readonly IDictionaryService _dictionaryService;

    public DictionaryController(IDictionaryService dictionaryService)
    {
        _dictionaryService = dictionaryService;
    }
    
    /// <summary>
    /// Get specialities list
    /// </summary>
    /// <response code="200">Specialties paged list retrieved</response>
    /// <response code="400">Invalid arguments for filtration/pagination</response>
    /// <response code="500">InternalServerError</response>
    [HttpGet("speciality")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SpecialtiesPagedListModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = null!)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
    public async Task<ActionResult<SpecialtiesPagedListModel>> GetSpecialities(
        [FromQuery] string? name,
        [FromQuery, DefaultValue(1)] int page,
        [FromQuery, DefaultValue(5)] int size
        )
    {
        if (size <= 0 || page <= 0)
        {
            return BadRequest(new ResponseModel
            {
                Status = "Error",
                Message = "Page and size values must be greater than 0"
            });
        }

        var specialities = await _dictionaryService.GetSpecialities(
            (string.IsNullOrEmpty(name)) ? "" : name,
            page,
            size);
        return Ok(specialities);
    }
    
    /// <summary>
    /// Search for diagnoses in ICD-10 dictionary
    /// </summary>
    /// <response code="200">Searching result extracted</response>
    /// <response code="400">Some fields in request are invalid</response>
    /// <response code="500">InternalServerError</response>
    [HttpGet("icd10")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Icd10SearchModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
    public async Task<ActionResult<Icd10SearchModel>> SearchForDiagnoses(
        [FromQuery] string? request,
        [FromQuery, DefaultValue(1)] int page,
        [FromQuery, DefaultValue(5)] int size
    )
    {
        if (size <= 0 || page <= 0)
        {
            return BadRequest(new ResponseModel
            {
                Status = "Error",
                Message = "Page and size values must be greater than 0"
            });
        }
        
        var diagnoses = await _dictionaryService.SearchForDiagnoses(
            string.IsNullOrEmpty(request) ? "" : request,
            page,
            size);
        return Ok(diagnoses);
    }
    
    /// <summary>
    /// Get root ICD-10 elements
    /// </summary>
    /// <response code="200">Root ICD-10 elements retrieved</response>
    /// <response code="500">InternalServerError</response>
    [HttpGet("icd10/roots")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Icd10RecordModel>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
    public async Task<ActionResult<List<Icd10RecordModel>>> GetRoots()
    {
        var roots = await _dictionaryService.GetRoots();
        return Ok(roots);
    }
}