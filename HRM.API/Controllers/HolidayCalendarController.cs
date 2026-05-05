using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.HolidayCalendar;
using HRM.Core.DTOs.LeaveAllotment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;

[ApiController]
[Authorize]
[Route("api/holidays")]
public class HolidayCalendarController : ControllerBase
{
    private readonly IHolidayCalendarService _holidayCalendarService;

    public HolidayCalendarController(IHolidayCalendarService holidayCalendarService)
    {
        _holidayCalendarService = holidayCalendarService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<HolidayResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateHolidayDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(BuildModelStateMessage()));
        }

        try
        {
            var created = await _holidayCalendarService.CreateAsync(dto);
            var location = Url.Action(nameof(GetById), new { id = created.Id }) ?? $"/api/holidays/{created.Id}";
            var response = ApiResponse<HolidayResponseDto>.SuccessResponse(created, "Holiday created successfully.");
            return Created(location, response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<HolidayResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<HolidayResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<HolidayResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPost("bulk")]
    [ProducesResponseType(typeof(ApiResponse<BulkCreateResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BulkCreate([FromBody] BulkCreateHolidayDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(BuildModelStateMessage()));
        }

        try
        {
            var result = await _holidayCalendarService.BulkCreateAsync(dto);
            return Ok(ApiResponse<BulkCreateResultDto>.SuccessResponse(result, "Bulk holiday creation processed."));
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
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<HolidayResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] HolidayFilterDto filter)
    {
        var items = await _holidayCalendarService.GetFilteredAsync(filter);
        return Ok(ApiResponse<IEnumerable<HolidayResponseDto>>.SuccessResponse(items, "Holidays retrieved successfully."));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<HolidayResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var holiday = await _holidayCalendarService.GetByIdAsync(id);
            return Ok(ApiResponse<HolidayResponseDto>.SuccessResponse(holiday, "Holiday retrieved successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<HolidayResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<HolidayResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("by-year/{year:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<HolidayResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByYear(int year, [FromQuery] int? branchId = null)
    {
        var items = await _holidayCalendarService.GetByYearAsync(year, branchId);
        return Ok(ApiResponse<IEnumerable<HolidayResponseDto>>.SuccessResponse(items, "Holidays retrieved successfully."));
    }

    [HttpGet("check")]
    [ProducesResponseType(typeof(ApiResponse<HolidayCheckResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckHoliday([FromQuery] DateTime date, [FromQuery] int? branchId = null)
    {
        var result = await _holidayCalendarService.IsHolidayAsync(date, branchId);
        return Ok(ApiResponse<HolidayCheckResultDto>.SuccessResponse(result, "Holiday check completed."));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<HolidayResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateHolidayDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(BuildModelStateMessage()));
        }

        try
        {
            var updated = await _holidayCalendarService.UpdateAsync(id, dto);
            return Ok(ApiResponse<HolidayResponseDto>.SuccessResponse(updated, "Holiday updated successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<HolidayResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<HolidayResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<HolidayResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _holidayCalendarService.DeleteAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null!,
                "Holiday deleted successfully. Consider deactivating instead of deleting to preserve historical records."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.FailureResponse(ex.Message));
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
