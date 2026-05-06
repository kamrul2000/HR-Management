using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.TaxExclusion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;

[ApiController]
[Authorize]
[Route("api/tax-exclusions")]
public class TaxExclusionController : ControllerBase
{
    private readonly ITaxExclusionService _exclusionService;

    public TaxExclusionController(ITaxExclusionService exclusionService)
    {
        _exclusionService = exclusionService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TaxExclusionResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateTaxExclusionDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(BuildModelStateMessage()));
        }

        try
        {
            var created = await _exclusionService.CreateAsync(dto);
            var location = Url.Action(nameof(GetById), new { id = created.Id }) ?? $"/api/tax-exclusions/{created.Id}";
            var response = ApiResponse<TaxExclusionResponseDto>.SuccessResponse(created, "Tax exclusion created successfully.");
            return Created(location, response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<TaxExclusionResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<TaxExclusionResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<TaxExclusionResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPost("{id:int}/attachment")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    [ProducesResponseType(typeof(ApiResponse<TaxExclusionResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadAttachment(int id, IFormFile file)
    {
        try
        {
            var updated = await _exclusionService.UploadAttachmentAsync(id, file);
            return Ok(ApiResponse<TaxExclusionResponseDto>.SuccessResponse(updated, "Attachment uploaded successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<TaxExclusionResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<TaxExclusionResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<TaxExclusionResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TaxExclusionResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllActive()
    {
        var items = await _exclusionService.GetAllActiveAsync();
        return Ok(ApiResponse<IEnumerable<TaxExclusionResponseDto>>.SuccessResponse(items, "Active tax exclusions retrieved successfully."));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<TaxExclusionResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var exclusion = await _exclusionService.GetByIdAsync(id);
            return Ok(ApiResponse<TaxExclusionResponseDto>.SuccessResponse(exclusion, "Tax exclusion retrieved successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<TaxExclusionResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<TaxExclusionResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("by-employee/{employeeId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TaxExclusionResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByEmployee(int employeeId)
    {
        try
        {
            var items = await _exclusionService.GetByEmployeeAsync(employeeId);
            return Ok(ApiResponse<IEnumerable<TaxExclusionResponseDto>>.SuccessResponse(items, "Tax exclusions retrieved successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<IEnumerable<TaxExclusionResponseDto>>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<IEnumerable<TaxExclusionResponseDto>>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("check/{employeeId:int}")]
    [ProducesResponseType(typeof(ApiResponse<TaxExclusionCheckDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Check(int employeeId)
    {
        var result = await _exclusionService.CheckExclusionAsync(employeeId);
        return Ok(ApiResponse<TaxExclusionCheckDto>.SuccessResponse(result, "Tax exclusion check completed."));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<TaxExclusionResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTaxExclusionDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(BuildModelStateMessage()));
        }

        try
        {
            var updated = await _exclusionService.UpdateAsync(id, dto);
            return Ok(ApiResponse<TaxExclusionResponseDto>.SuccessResponse(updated, "Tax exclusion updated successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<TaxExclusionResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<TaxExclusionResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<TaxExclusionResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _exclusionService.DeleteAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null!, "Tax exclusion deleted successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    private string BuildModelStateMessage()
    {
        return string.Join(" ",
            ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
    }
}
