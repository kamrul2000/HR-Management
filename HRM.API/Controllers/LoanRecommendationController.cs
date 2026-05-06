using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.LoanRecommendation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;

[ApiController]
[Authorize]
[Route("api/loan-recommendations")]
public class LoanRecommendationController : ControllerBase
{
    private readonly ILoanRecommendationService _recommendationService;

    public LoanRecommendationController(ILoanRecommendationService recommendationService)
    {
        _recommendationService = recommendationService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<LoanRecommendationResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Recommend([FromBody] CreateRecommendationDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(BuildModelStateMessage()));
        }

        try
        {
            var created = await _recommendationService.RecommendAsync(dto);
            var location = Url.Action(nameof(GetById), new { id = created.Id }) ?? $"/api/loan-recommendations/{created.Id}";
            var response = ApiResponse<LoanRecommendationResponseDto>.SuccessResponse(created, "Loan recommendation submitted successfully.");
            return Created(location, response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<LoanRecommendationResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<LoanRecommendationResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<LoanRecommendationResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<LoanRecommendationResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var items = await _recommendationService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<LoanRecommendationResponseDto>>.SuccessResponse(items, "Loan recommendations retrieved successfully."));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<LoanRecommendationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var recommendation = await _recommendationService.GetByIdAsync(id);
            return Ok(ApiResponse<LoanRecommendationResponseDto>.SuccessResponse(recommendation, "Loan recommendation retrieved successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<LoanRecommendationResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<LoanRecommendationResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("by-application/{loanApplicationId:int}")]
    [ProducesResponseType(typeof(ApiResponse<LoanRecommendationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByLoanApplication(int loanApplicationId)
    {
        try
        {
            var recommendation = await _recommendationService.GetByLoanApplicationAsync(loanApplicationId);
            return Ok(ApiResponse<LoanRecommendationResponseDto>.SuccessResponse(recommendation, "Loan recommendation retrieved successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<LoanRecommendationResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<LoanRecommendationResponseDto>.FailureResponse(ex.Message));
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
