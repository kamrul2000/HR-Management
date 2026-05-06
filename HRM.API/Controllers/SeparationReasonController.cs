using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.SeparationReason;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;

[ApiController]
[Authorize]
[Route("api/separation-reasons")]
public class SeparationReasonController : ControllerBase
{
    private readonly ISeparationReasonService _reasonService;

    public SeparationReasonController(ISeparationReasonService reasonService)
    {
        _reasonService = reasonService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SeparationReasonResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateSeparationReasonDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(BuildModelStateMessage()));
        }

        try
        {
            var created = await _reasonService.CreateAsync(dto);
            var location = Url.Action(nameof(GetById), new { id = created.Id }) ?? $"/api/separation-reasons/{created.Id}";
            var response = ApiResponse<SeparationReasonResponseDto>.SuccessResponse(created, "Separation reason created successfully.");
            return Created(location, response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<SeparationReasonResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<SeparationReasonResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SeparationReasonResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var reasons = await _reasonService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<SeparationReasonResponseDto>>.SuccessResponse(
            reasons, "Separation reasons retrieved successfully."));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SeparationReasonResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var reason = await _reasonService.GetByIdAsync(id);
            return Ok(ApiResponse<SeparationReasonResponseDto>.SuccessResponse(reason, "Separation reason retrieved successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<SeparationReasonResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<SeparationReasonResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("by-type/{separationType}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SeparationReasonResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetBySeparationType(string separationType)
    {
        try
        {
            var reasons = await _reasonService.GetBySeparationTypeAsync(separationType);
            return Ok(ApiResponse<IEnumerable<SeparationReasonResponseDto>>.SuccessResponse(
                reasons, "Separation reasons retrieved successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<IEnumerable<SeparationReasonResponseDto>>.FailureResponse(ex.Message));
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SeparationReasonResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSeparationReasonDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(BuildModelStateMessage()));
        }

        try
        {
            var updated = await _reasonService.UpdateAsync(id, dto);
            return Ok(ApiResponse<SeparationReasonResponseDto>.SuccessResponse(updated, "Separation reason updated successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<SeparationReasonResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<SeparationReasonResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<SeparationReasonResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _reasonService.DeleteAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null!, "Separation reason deleted successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
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
