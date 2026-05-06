using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.Employee;
using HRM.Core.DTOs.EmployeeLoan;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;

[ApiController]
[Authorize]
[Route("api/employee-loans")]
public class EmployeeLoanController : ControllerBase
{
    private readonly IEmployeeLoanService _loanService;

    public EmployeeLoanController(IEmployeeLoanService loanService)
    {
        _loanService = loanService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<EmployeeLoanResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeLoanDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(BuildModelStateMessage()));
        }

        try
        {
            var created = await _loanService.CreateAsync(dto);
            var location = Url.Action(nameof(GetById), new { id = created.Id }) ?? $"/api/employee-loans/{created.Id}";
            var response = ApiResponse<EmployeeLoanResponseDto>.SuccessResponse(created, "Employee loan disbursed successfully.");
            return Created(location, response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<EmployeeLoanResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<EmployeeLoanResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<EmployeeLoanResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResultDto<EmployeeLoanResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] EmployeeLoanFilterDto filter)
    {
        var result = await _loanService.GetFilteredAsync(filter);
        return Ok(ApiResponse<PagedResultDto<EmployeeLoanResponseDto>>.SuccessResponse(result, "Employee loans retrieved successfully."));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeLoanResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var loan = await _loanService.GetByIdAsync(id);
            return Ok(ApiResponse<EmployeeLoanResponseDto>.SuccessResponse(loan, "Employee loan retrieved successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<EmployeeLoanResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<EmployeeLoanResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("by-employee/{employeeId:int}")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeLoanResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByEmployee(int employeeId)
    {
        try
        {
            var loan = await _loanService.GetByEmployeeAsync(employeeId);
            return Ok(ApiResponse<EmployeeLoanResponseDto>.SuccessResponse(loan, "Employee loan retrieved successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<EmployeeLoanResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<EmployeeLoanResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPut("{id:int}/complete")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeLoanResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Complete(int id)
    {
        try
        {
            var updated = await _loanService.MarkCompletedAsync(id);
            return Ok(ApiResponse<EmployeeLoanResponseDto>.SuccessResponse(updated, "Loan marked as completed."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<EmployeeLoanResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<EmployeeLoanResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<EmployeeLoanResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPut("{id:int}/default")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeLoanResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Default(int id, [FromQuery] string reason)
    {
        try
        {
            var updated = await _loanService.MarkDefaultedAsync(id, reason);
            return Ok(ApiResponse<EmployeeLoanResponseDto>.SuccessResponse(updated, "Loan marked as defaulted."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<EmployeeLoanResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<EmployeeLoanResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<EmployeeLoanResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPut("{id:int}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeLoanResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(int id, [FromQuery] string reason)
    {
        try
        {
            var updated = await _loanService.CancelAsync(id, reason);
            return Ok(ApiResponse<EmployeeLoanResponseDto>.SuccessResponse(updated, "Loan cancelled."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<EmployeeLoanResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<EmployeeLoanResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<EmployeeLoanResponseDto>.FailureResponse(ex.Message));
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
