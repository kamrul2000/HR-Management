using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.LeaveAllotment;
using HRM.Core.DTOs.PfInterest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;

[ApiController]
[Authorize]
[Route("api/pf-interest")]
public class PfInterestController : ControllerBase
{
    private readonly IPfInterestService _interestService;

    public PfInterestController(IPfInterestService interestService)
    {
        _interestService = interestService;
    }

    [HttpPost("rates")]
    [ProducesResponseType(typeof(ApiResponse<PfInterestRateResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateRate([FromBody] CreatePfInterestRateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(BuildModelStateMessage()));
        }

        try
        {
            var created = await _interestService.CreateRateAsync(dto);
            var location = Url.Action(nameof(GetRateByFiscalYear), new { fiscalYear = created.FiscalYear })
                          ?? $"/api/pf-interest/rates/{created.FiscalYear}";
            var response = ApiResponse<PfInterestRateResponseDto>.SuccessResponse(created, "PF interest rate created successfully.");
            return Created(location, response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<PfInterestRateResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<PfInterestRateResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("rates")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PfInterestRateResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllRates()
    {
        var rates = await _interestService.GetAllRatesAsync();
        return Ok(ApiResponse<IEnumerable<PfInterestRateResponseDto>>.SuccessResponse(
            rates, "PF interest rates retrieved successfully."));
    }

    [HttpGet("rates/{fiscalYear}")]
    [ProducesResponseType(typeof(ApiResponse<PfInterestRateResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRateByFiscalYear(string fiscalYear)
    {
        try
        {
            var rate = await _interestService.GetRateByFiscalYearAsync(fiscalYear);
            return Ok(ApiResponse<PfInterestRateResponseDto>.SuccessResponse(rate, "PF interest rate retrieved successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<PfInterestRateResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPost("compute")]
    [ProducesResponseType(typeof(ApiResponse<EmployeePfInterestResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Compute([FromBody] ComputePfInterestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(BuildModelStateMessage()));
        }

        try
        {
            var created = await _interestService.ComputeAsync(dto);
            var location = Url.Action(nameof(GetById), new { id = created.Id }) ?? $"/api/pf-interest/{created.Id}";
            var response = ApiResponse<EmployeePfInterestResponseDto>.SuccessResponse(created, "PF interest computed successfully.");
            return Created(location, response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<EmployeePfInterestResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<EmployeePfInterestResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<EmployeePfInterestResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPost("bulk-compute")]
    [ProducesResponseType(typeof(ApiResponse<BulkCreateResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BulkCompute([FromBody] BulkComputePfInterestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(BuildModelStateMessage()));
        }

        try
        {
            var result = await _interestService.BulkComputeAsync(dto);
            return Ok(ApiResponse<BulkCreateResultDto>.SuccessResponse(result, "Bulk PF interest computation completed."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<BulkCreateResultDto>.FailureResponse(ex.Message));
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

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<EmployeePfInterestResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var record = await _interestService.GetByIdAsync(id);
            return Ok(ApiResponse<EmployeePfInterestResponseDto>.SuccessResponse(record, "PF interest record retrieved successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<EmployeePfInterestResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<EmployeePfInterestResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("by-employee/{employeeId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<EmployeePfInterestResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByEmployee(int employeeId)
    {
        try
        {
            var items = await _interestService.GetByEmployeeAsync(employeeId);
            return Ok(ApiResponse<IEnumerable<EmployeePfInterestResponseDto>>.SuccessResponse(
                items, "PF interest history retrieved successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<IEnumerable<EmployeePfInterestResponseDto>>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<IEnumerable<EmployeePfInterestResponseDto>>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("report/{fiscalYear}")]
    [ProducesResponseType(typeof(ApiResponse<PfInterestReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReport(string fiscalYear, [FromQuery] int? branchId = null)
    {
        try
        {
            var report = await _interestService.GetReportAsync(fiscalYear, branchId);
            return Ok(ApiResponse<PfInterestReportDto>.SuccessResponse(report, "PF interest report generated successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<PfInterestReportDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<PfInterestReportDto>.FailureResponse(ex.Message));
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
