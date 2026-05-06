using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.SalaryHead;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;

[ApiController]
[Authorize]
[Route("api/salary-heads")]
public class SalaryHeadController : ControllerBase
{
    private readonly ISalaryHeadService _salaryHeadService;

    public SalaryHeadController(ISalaryHeadService salaryHeadService)
    {
        _salaryHeadService = salaryHeadService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SalaryHeadResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateSalaryHeadDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(BuildModelStateMessage()));
        }

        try
        {
            var created = await _salaryHeadService.CreateAsync(dto);
            var location = Url.Action(nameof(GetById), new { id = created.Id }) ?? $"/api/salary-heads/{created.Id}";
            var response = ApiResponse<SalaryHeadResponseDto>.SuccessResponse(created, "Salary head created successfully.");
            return Created(location, response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<SalaryHeadResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<SalaryHeadResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<SalaryHeadResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SalaryHeadResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var items = await _salaryHeadService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<SalaryHeadResponseDto>>.SuccessResponse(items, "Salary heads retrieved successfully."));
    }

    [HttpGet("earnings")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SalaryHeadResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEarnings()
    {
        var items = await _salaryHeadService.GetEarningsAsync();
        return Ok(ApiResponse<IEnumerable<SalaryHeadResponseDto>>.SuccessResponse(items, "Earning heads retrieved successfully."));
    }

    [HttpGet("deductions")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SalaryHeadResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeductions()
    {
        var items = await _salaryHeadService.GetDeductionsAsync();
        return Ok(ApiResponse<IEnumerable<SalaryHeadResponseDto>>.SuccessResponse(items, "Deduction heads retrieved successfully."));
    }

    [HttpGet("summary")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SalaryHeadSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveHeadsSummary()
    {
        var items = await _salaryHeadService.GetActiveHeadsSummaryAsync();
        return Ok(ApiResponse<IEnumerable<SalaryHeadSummaryDto>>.SuccessResponse(items, "Active salary heads retrieved successfully."));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SalaryHeadResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var head = await _salaryHeadService.GetByIdAsync(id);
            return Ok(ApiResponse<SalaryHeadResponseDto>.SuccessResponse(head, "Salary head retrieved successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<SalaryHeadResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<SalaryHeadResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SalaryHeadResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSalaryHeadDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(BuildModelStateMessage()));
        }

        try
        {
            var updated = await _salaryHeadService.UpdateAsync(id, dto);
            return Ok(ApiResponse<SalaryHeadResponseDto>.SuccessResponse(updated, "Salary head updated successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<SalaryHeadResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<SalaryHeadResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<SalaryHeadResponseDto>.FailureResponse(ex.Message));
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
            await _salaryHeadService.DeleteAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null!, "Salary head deleted successfully."));
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
