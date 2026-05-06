using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.LoanApproval;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;

[ApiController]
[Authorize]
[Route("api/loan-approvals")]
public class LoanApprovalController : ControllerBase
{
    private readonly ILoanApprovalService _loanApprovalService;

    public LoanApprovalController(ILoanApprovalService loanApprovalService)
    {
        _loanApprovalService = loanApprovalService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<LoanApprovalResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Process([FromBody] CreateLoanApprovalDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(BuildModelStateMessage()));
        }

        try
        {
            var created = await _loanApprovalService.ProcessAsync(dto);
            var location = Url.Action(nameof(GetById), new { id = created.Id }) ?? $"/api/loan-approvals/{created.Id}";
            var response = ApiResponse<LoanApprovalResponseDto>.SuccessResponse(created, "Loan approval decision recorded.");
            return Created(location, response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<LoanApprovalResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<LoanApprovalResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<LoanApprovalResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<LoanApprovalResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var items = await _loanApprovalService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<LoanApprovalResponseDto>>.SuccessResponse(items, "Loan approvals retrieved successfully."));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<LoanApprovalResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var approval = await _loanApprovalService.GetByIdAsync(id);
            return Ok(ApiResponse<LoanApprovalResponseDto>.SuccessResponse(approval, "Loan approval retrieved successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<LoanApprovalResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<LoanApprovalResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("by-application/{loanApplicationId:int}")]
    [ProducesResponseType(typeof(ApiResponse<LoanApprovalResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByLoanApplication(int loanApplicationId)
    {
        try
        {
            var approval = await _loanApprovalService.GetByLoanApplicationAsync(loanApplicationId);
            return Ok(ApiResponse<LoanApprovalResponseDto>.SuccessResponse(approval, "Loan approval retrieved successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<LoanApprovalResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<LoanApprovalResponseDto>.FailureResponse(ex.Message));
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
