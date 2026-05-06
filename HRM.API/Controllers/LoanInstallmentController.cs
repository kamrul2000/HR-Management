using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.Employee;
using HRM.Core.DTOs.LoanInstallment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;

[ApiController]
[Authorize]
[Route("api/loan-installments")]
public class LoanInstallmentController : ControllerBase
{
    private readonly ILoanInstallmentService _installmentService;

    public LoanInstallmentController(ILoanInstallmentService installmentService)
    {
        _installmentService = installmentService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResultDto<LoanInstallmentResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] InstallmentFilterDto filter)
    {
        var result = await _installmentService.GetFilteredAsync(filter);
        return Ok(ApiResponse<PagedResultDto<LoanInstallmentResponseDto>>.SuccessResponse(result, "Loan installments retrieved successfully."));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<LoanInstallmentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var installment = await _installmentService.GetByIdAsync(id);
            return Ok(ApiResponse<LoanInstallmentResponseDto>.SuccessResponse(installment, "Loan installment retrieved successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<LoanInstallmentResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<LoanInstallmentResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("by-loan/{employeeLoanId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<LoanInstallmentResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByLoan(int employeeLoanId)
    {
        try
        {
            var items = await _installmentService.GetByLoanAsync(employeeLoanId);
            return Ok(ApiResponse<IEnumerable<LoanInstallmentResponseDto>>.SuccessResponse(items, "Loan installments retrieved successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<IEnumerable<LoanInstallmentResponseDto>>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<IEnumerable<LoanInstallmentResponseDto>>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("pending/{employeeId:int}/{year:int}/{month:int}")]
    [ProducesResponseType(typeof(ApiResponse<PendingInstallmentDto?>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPending(int employeeId, int year, int month)
    {
        var pending = await _installmentService.GetPendingInstallmentAsync(employeeId, year, month);
        return Ok(ApiResponse<PendingInstallmentDto?>.SuccessResponse(pending,
            pending is null ? "No pending installment due for this period." : "Pending installment retrieved successfully."));
    }

    [HttpPut("{id:int}/pay")]
    [ProducesResponseType(typeof(ApiResponse<LoanInstallmentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Pay(int id, [FromBody] ProcessInstallmentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(BuildModelStateMessage()));
        }

        try
        {
            var updated = await _installmentService.ProcessPaymentAsync(id, dto);
            return Ok(ApiResponse<LoanInstallmentResponseDto>.SuccessResponse(updated, "Installment payment processed."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<LoanInstallmentResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<LoanInstallmentResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<LoanInstallmentResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPut("{id:int}/skip")]
    [ProducesResponseType(typeof(ApiResponse<LoanInstallmentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Skip(int id, [FromBody] SkipInstallmentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(BuildModelStateMessage()));
        }

        try
        {
            var updated = await _installmentService.SkipAsync(id, dto);
            return Ok(ApiResponse<LoanInstallmentResponseDto>.SuccessResponse(updated, "Installment skipped."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<LoanInstallmentResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<LoanInstallmentResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<LoanInstallmentResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPut("{id:int}/reinstate")]
    [ProducesResponseType(typeof(ApiResponse<LoanInstallmentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reinstate(int id)
    {
        try
        {
            var updated = await _installmentService.ReinstateAsync(id);
            return Ok(ApiResponse<LoanInstallmentResponseDto>.SuccessResponse(updated, "Installment reinstated to pending."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<LoanInstallmentResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<LoanInstallmentResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<LoanInstallmentResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPost("mark-overdue")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkOverdue([FromQuery] int year, [FromQuery] int month)
    {
        var count = await _installmentService.MarkOverdueAsync(year, month);
        return Ok(ApiResponse<object>.SuccessResponse(new { updated = count },
            $"{count} installment(s) marked as overdue."));
    }

    private string BuildModelStateMessage()
    {
        return string.Join(" ",
            ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
    }
}
