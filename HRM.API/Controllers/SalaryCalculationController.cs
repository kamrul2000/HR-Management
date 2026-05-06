using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.Employee;
using HRM.Core.DTOs.LeaveAllotment;
using HRM.Core.DTOs.SalaryCalculation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;

[ApiController]
[Authorize]
[Route("api/salary-calculations")]
public class SalaryCalculationController : ControllerBase
{
    private readonly ISalaryCalculationService _salaryCalculationService;

    public SalaryCalculationController(ISalaryCalculationService salaryCalculationService)
    {
        _salaryCalculationService = salaryCalculationService;
    }

    [HttpPost("run")]
    [ProducesResponseType(typeof(ApiResponse<SalaryCalculationResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Run([FromBody] RunSalaryCalculationDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(BuildModelStateMessage()));
        }

        try
        {
            var created = await _salaryCalculationService.CalculateAsync(dto);
            var location = Url.Action(nameof(GetById), new { id = created.Id }) ?? $"/api/salary-calculations/{created.Id}";
            var response = ApiResponse<SalaryCalculationResponseDto>.SuccessResponse(created, "Salary calculation completed successfully.");
            return Created(location, response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<SalaryCalculationResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<SalaryCalculationResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<SalaryCalculationResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPost("bulk-run")]
    [ProducesResponseType(typeof(ApiResponse<BulkCreateResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> BulkRun([FromBody] BulkRunSalaryDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(BuildModelStateMessage()));
        }

        try
        {
            var result = await _salaryCalculationService.BulkCalculateAsync(dto);
            return Ok(ApiResponse<BulkCreateResultDto>.SuccessResponse(result, "Bulk salary calculation processed."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<BulkCreateResultDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<BulkCreateResultDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResultDto<SalaryCalculationResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] SalaryCalculationFilterDto filter)
    {
        var result = await _salaryCalculationService.GetFilteredAsync(filter);
        return Ok(ApiResponse<PagedResultDto<SalaryCalculationResponseDto>>.SuccessResponse(result, "Salary calculations retrieved successfully."));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SalaryCalculationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var calculation = await _salaryCalculationService.GetByIdAsync(id);
            return Ok(ApiResponse<SalaryCalculationResponseDto>.SuccessResponse(calculation, "Salary calculation retrieved successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<SalaryCalculationResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<SalaryCalculationResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("employee/{employeeId:int}/{year:int}/{month:int}")]
    [ProducesResponseType(typeof(ApiResponse<SalaryCalculationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByEmployeeMonth(int employeeId, int year, int month)
    {
        try
        {
            var calculation = await _salaryCalculationService.GetByEmployeeMonthAsync(employeeId, year, month);
            return Ok(ApiResponse<SalaryCalculationResponseDto>.SuccessResponse(calculation, "Salary calculation retrieved successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<SalaryCalculationResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<SalaryCalculationResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("report/{year:int}/{month:int}")]
    [ProducesResponseType(typeof(ApiResponse<MonthlySalaryReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReport(int year, int month, [FromQuery] int? branchId = null)
    {
        try
        {
            var report = await _salaryCalculationService.GetMonthlyReportAsync(year, month, branchId);
            return Ok(ApiResponse<MonthlySalaryReportDto>.SuccessResponse(report, "Monthly salary report retrieved successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<MonthlySalaryReportDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<MonthlySalaryReportDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPut("{id:int}/finalize")]
    [ProducesResponseType(typeof(ApiResponse<SalaryCalculationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Finalize(int id)
    {
        try
        {
            var updated = await _salaryCalculationService.FinalizeAsync(id);
            return Ok(ApiResponse<SalaryCalculationResponseDto>.SuccessResponse(updated, "Salary calculation finalized."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<SalaryCalculationResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<SalaryCalculationResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<SalaryCalculationResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPut("{id:int}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<SalaryCalculationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(int id, [FromQuery] string reason)
    {
        try
        {
            var updated = await _salaryCalculationService.CancelAsync(id, reason);
            return Ok(ApiResponse<SalaryCalculationResponseDto>.SuccessResponse(updated, "Salary calculation cancelled."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<SalaryCalculationResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<SalaryCalculationResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<SalaryCalculationResponseDto>.FailureResponse(ex.Message));
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
