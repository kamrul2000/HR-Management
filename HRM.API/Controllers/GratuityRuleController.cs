using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.GratuitySetup;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;

[ApiController]
[Authorize]
[Route("api/gratuity-rules")]
public class GratuityRuleController : ControllerBase
{
    private readonly IGratuityRuleService _ruleService;

    public GratuityRuleController(IGratuityRuleService ruleService)
    {
        _ruleService = ruleService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<GratuityRuleResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateGratuityRuleDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(BuildModelStateMessage()));
        }

        try
        {
            var created = await _ruleService.CreateAsync(dto);
            var location = Url.Action(nameof(GetById), new { id = created.Id }) ?? $"/api/gratuity-rules/{created.Id}";
            var response = ApiResponse<GratuityRuleResponseDto>.SuccessResponse(created, "Gratuity rule created successfully.");
            return Created(location, response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<GratuityRuleResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<GratuityRuleResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<GratuityRuleResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var rules = await _ruleService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<GratuityRuleResponseDto>>.SuccessResponse(rules, "Gratuity rules retrieved successfully."));
    }

    [HttpGet("active")]
    [ProducesResponseType(typeof(ApiResponse<GratuityRuleResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetActive()
    {
        try
        {
            var rule = await _ruleService.GetActiveAsync();
            return Ok(ApiResponse<GratuityRuleResponseDto>.SuccessResponse(rule, "Active gratuity rule retrieved successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<GratuityRuleResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<GratuityRuleResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var rule = await _ruleService.GetByIdAsync(id);
            return Ok(ApiResponse<GratuityRuleResponseDto>.SuccessResponse(rule, "Gratuity rule retrieved successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<GratuityRuleResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<GratuityRuleResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<GratuityRuleResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateGratuityRuleDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(BuildModelStateMessage()));
        }

        try
        {
            var updated = await _ruleService.UpdateAsync(id, dto);
            return Ok(ApiResponse<GratuityRuleResponseDto>.SuccessResponse(updated, "Gratuity rule updated successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<GratuityRuleResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<GratuityRuleResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<GratuityRuleResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPost("preview")]
    [ProducesResponseType(typeof(ApiResponse<GratuityPreviewResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Preview([FromBody] GratuityPreviewDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(BuildModelStateMessage()));
        }

        try
        {
            var result = await _ruleService.PreviewAsync(dto);
            return Ok(ApiResponse<GratuityPreviewResultDto>.SuccessResponse(result, "Gratuity preview generated successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<GratuityPreviewResultDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<GratuityPreviewResultDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<GratuityPreviewResultDto>.FailureResponse(ex.Message));
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
            await _ruleService.DeleteAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null!, "Gratuity rule deleted successfully."));
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
