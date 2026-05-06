using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.GratuityCalculation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;

[ApiController]
[Authorize]
[Route("api/gratuity-calculations")]
public class GratuityCalculationController : ControllerBase
{
    private readonly IGratuityCalculationService _calculationService;

    public GratuityCalculationController(IGratuityCalculationService calculationService)
    {
        _calculationService = calculationService;
    }

    [HttpPost("compute")]
    [ProducesResponseType(typeof(ApiResponse<GratuityCalculationResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Compute([FromBody] ComputeGratuityDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(BuildModelStateMessage()));
        }

        try
        {
            var created = await _calculationService.ComputeAsync(dto);
            var location = Url.Action(nameof(GetById), new { id = created.Id }) ?? $"/api/gratuity-calculations/{created.Id}";
            var response = ApiResponse<GratuityCalculationResponseDto>.SuccessResponse(created, "Gratuity calculation computed successfully.");
            return Created(location, response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<GratuityCalculationResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<GratuityCalculationResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<GratuityCalculationResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<GratuityCalculationResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var items = await _calculationService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<GratuityCalculationResponseDto>>.SuccessResponse(
            items, "Gratuity calculations retrieved successfully."));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<GratuityCalculationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var calc = await _calculationService.GetByIdAsync(id);
            return Ok(ApiResponse<GratuityCalculationResponseDto>.SuccessResponse(calc, "Gratuity calculation retrieved successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<GratuityCalculationResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<GratuityCalculationResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("by-employee/{employeeId:int}")]
    [ProducesResponseType(typeof(ApiResponse<GratuityCalculationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByEmployee(int employeeId)
    {
        try
        {
            var calc = await _calculationService.GetByEmployeeAsync(employeeId);
            return Ok(ApiResponse<GratuityCalculationResponseDto>.SuccessResponse(calc, "Gratuity calculation retrieved successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<GratuityCalculationResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<GratuityCalculationResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("report")]
    [ProducesResponseType(typeof(ApiResponse<GratuityReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReport([FromQuery] int? branchId = null, [FromQuery] string? status = null)
    {
        try
        {
            var report = await _calculationService.GetReportAsync(branchId, status);
            return Ok(ApiResponse<GratuityReportDto>.SuccessResponse(report, "Gratuity report generated successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<GratuityReportDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<GratuityReportDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPut("{id:int}/finalize")]
    [ProducesResponseType(typeof(ApiResponse<GratuityCalculationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Finalize(int id)
    {
        try
        {
            var updated = await _calculationService.FinalizeAsync(id);
            return Ok(ApiResponse<GratuityCalculationResponseDto>.SuccessResponse(updated, "Gratuity calculation finalized successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<GratuityCalculationResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<GratuityCalculationResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<GratuityCalculationResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPut("{id:int}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<GratuityCalculationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(int id, [FromQuery] string reason)
    {
        try
        {
            var updated = await _calculationService.CancelAsync(id, reason);
            return Ok(ApiResponse<GratuityCalculationResponseDto>.SuccessResponse(updated, "Gratuity calculation cancelled successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<GratuityCalculationResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<GratuityCalculationResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<GratuityCalculationResponseDto>.FailureResponse(ex.Message));
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
