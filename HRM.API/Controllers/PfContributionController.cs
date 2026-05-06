using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.Employee;
using HRM.Core.DTOs.LeaveAllotment;
using HRM.Core.DTOs.PfContribution;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;

[ApiController]
[Authorize]
[Route("api/pf")]
public class PfContributionController : ControllerBase
{
    private readonly IPfContributionService _pfService;

    public PfContributionController(IPfContributionService pfService)
    {
        _pfService = pfService;
    }

    [HttpPost("rules")]
    [ProducesResponseType(typeof(ApiResponse<PfRuleResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateRule([FromBody] CreatePfRuleDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(BuildModelStateMessage()));
        }

        try
        {
            var created = await _pfService.CreateRuleAsync(dto);
            var location = Url.Action(nameof(GetRuleById), new { id = created.Id }) ?? $"/api/pf/rules/{created.Id}";
            var response = ApiResponse<PfRuleResponseDto>.SuccessResponse(created, "PF rule created successfully.");
            return Created(location, response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<PfRuleResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<PfRuleResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<PfRuleResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("rules")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PfRuleResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllRules()
    {
        var rules = await _pfService.GetAllRulesAsync();
        return Ok(ApiResponse<IEnumerable<PfRuleResponseDto>>.SuccessResponse(rules, "PF rules retrieved successfully."));
    }

    [HttpGet("rules/active")]
    [ProducesResponseType(typeof(ApiResponse<PfRuleResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetActiveRule()
    {
        try
        {
            var rule = await _pfService.GetActiveRuleAsync();
            return Ok(ApiResponse<PfRuleResponseDto>.SuccessResponse(rule, "Active PF rule retrieved successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<PfRuleResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("rules/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<PfRuleResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRuleById(int id)
    {
        try
        {
            var rule = await _pfService.GetRuleByIdAsync(id);
            return Ok(ApiResponse<PfRuleResponseDto>.SuccessResponse(rule, "PF rule retrieved successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<PfRuleResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<PfRuleResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPut("rules/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<PfRuleResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRule(int id, [FromBody] UpdatePfRuleDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(BuildModelStateMessage()));
        }

        try
        {
            var updated = await _pfService.UpdateRuleAsync(id, dto);
            return Ok(ApiResponse<PfRuleResponseDto>.SuccessResponse(updated, "PF rule updated successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<PfRuleResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<PfRuleResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<PfRuleResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPost("compute/{employeeId:int}/{year:int}/{month:int}")]
    [ProducesResponseType(typeof(ApiResponse<EmployeePfContributionResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Compute(int employeeId, int year, int month)
    {
        try
        {
            var created = await _pfService.ComputeAsync(employeeId, year, month);
            var location = Url.Action(nameof(GetContributionById), new { id = created.Id }) ?? $"/api/pf/contributions/{created.Id}";
            var response = ApiResponse<EmployeePfContributionResponseDto>.SuccessResponse(created, "PF contribution computed successfully.");
            return Created(location, response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<EmployeePfContributionResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<EmployeePfContributionResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<EmployeePfContributionResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPost("bulk-compute")]
    [ProducesResponseType(typeof(ApiResponse<BulkCreateResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> BulkCompute(
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] int? branchId = null)
    {
        try
        {
            var result = await _pfService.BulkComputeAsync(year, month, branchId);
            return Ok(ApiResponse<BulkCreateResultDto>.SuccessResponse(result, "Bulk PF computation completed."));
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

    [HttpGet("contributions")]
    [ProducesResponseType(typeof(ApiResponse<PagedResultDto<EmployeePfContributionResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFiltered([FromQuery] PfContributionFilterDto filter)
    {
        var result = await _pfService.GetFilteredAsync(filter);
        return Ok(ApiResponse<PagedResultDto<EmployeePfContributionResponseDto>>.SuccessResponse(
            result, "PF contributions retrieved successfully."));
    }

    [HttpGet("contributions/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<EmployeePfContributionResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetContributionById(int id)
    {
        try
        {
            var contribution = await _pfService.GetContributionByIdAsync(id);
            return Ok(ApiResponse<EmployeePfContributionResponseDto>.SuccessResponse(contribution, "PF contribution retrieved successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<EmployeePfContributionResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<EmployeePfContributionResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("contributions/by-employee/{employeeId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<EmployeePfContributionResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByEmployee(int employeeId, [FromQuery] int? year = null)
    {
        try
        {
            var items = await _pfService.GetByEmployeeAsync(employeeId, year);
            return Ok(ApiResponse<IEnumerable<EmployeePfContributionResponseDto>>.SuccessResponse(
                items, "PF contributions retrieved successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<IEnumerable<EmployeePfContributionResponseDto>>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<IEnumerable<EmployeePfContributionResponseDto>>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("report/{year:int}/{month:int}")]
    [ProducesResponseType(typeof(ApiResponse<PfMonthlyReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetReport(int year, int month, [FromQuery] int? branchId = null)
    {
        try
        {
            var report = await _pfService.GetMonthlyReportAsync(year, month, branchId);
            return Ok(ApiResponse<PfMonthlyReportDto>.SuccessResponse(report, "PF monthly report generated successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<PfMonthlyReportDto>.FailureResponse(ex.Message));
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
