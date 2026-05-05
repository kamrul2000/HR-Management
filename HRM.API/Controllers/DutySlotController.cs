using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.DutySlot;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;

[ApiController]
[Authorize]
[Route("api/duty-slots")]
public class DutySlotController : ControllerBase
{
    private readonly IDutySlotService _dutySlotService;

    public DutySlotController(IDutySlotService dutySlotService)
    {
        _dutySlotService = dutySlotService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<DutySlotResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateDutySlotDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(BuildModelStateMessage()));
        }

        try
        {
            var created = await _dutySlotService.CreateAsync(dto);
            var location = Url.Action(nameof(GetById), new { id = created.Id }) ?? $"/api/duty-slots/{created.Id}";
            var response = ApiResponse<DutySlotResponseDto>.SuccessResponse(created, "Duty slot created successfully.");
            return Created(location, response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<DutySlotResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<DutySlotResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<DutySlotResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var items = await _dutySlotService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<DutySlotResponseDto>>.SuccessResponse(items, "Duty slots retrieved successfully."));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<DutySlotResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var dutySlot = await _dutySlotService.GetByIdAsync(id);
            return Ok(ApiResponse<DutySlotResponseDto>.SuccessResponse(dutySlot, "Duty slot retrieved successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<DutySlotResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<DutySlotResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<DutySlotResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDutySlotDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(BuildModelStateMessage()));
        }

        try
        {
            var updated = await _dutySlotService.UpdateAsync(id, dto);
            return Ok(ApiResponse<DutySlotResponseDto>.SuccessResponse(updated, "Duty slot updated successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<DutySlotResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<DutySlotResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<DutySlotResponseDto>.FailureResponse(ex.Message));
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
            await _dutySlotService.DeleteAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null!, "Duty slot deleted successfully."));
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
