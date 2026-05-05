using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.LeaveAllotment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;

[ApiController]
[Authorize]
[Route("api/leave-allotments")]
public class LeaveAllotmentController : ControllerBase
{
    private readonly ILeaveAllotmentService _leaveAllotmentService;

    public LeaveAllotmentController(ILeaveAllotmentService leaveAllotmentService)
    {
        _leaveAllotmentService = leaveAllotmentService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<LeaveAllotmentResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateLeaveAllotmentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(BuildModelStateMessage()));
        }

        try
        {
            var created = await _leaveAllotmentService.CreateAsync(dto);
            var location = Url.Action(nameof(GetById), new { id = created.Id }) ?? $"/api/leave-allotments/{created.Id}";
            var response = ApiResponse<LeaveAllotmentResponseDto>.SuccessResponse(created, "Leave allotment created successfully.");
            return Created(location, response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<LeaveAllotmentResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<LeaveAllotmentResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<LeaveAllotmentResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPost("bulk")]
    [ProducesResponseType(typeof(ApiResponse<BulkCreateResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BulkCreate([FromBody] BulkCreateLeaveAllotmentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(BuildModelStateMessage()));
        }

        try
        {
            var result = await _leaveAllotmentService.BulkCreateAsync(dto);
            return Ok(ApiResponse<BulkCreateResultDto>.SuccessResponse(result, "Bulk allotment processed."));
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
    [ProducesResponseType(typeof(ApiResponse<LeaveAllotmentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var allotment = await _leaveAllotmentService.GetByIdAsync(id);
            return Ok(ApiResponse<LeaveAllotmentResponseDto>.SuccessResponse(allotment, "Leave allotment retrieved successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<LeaveAllotmentResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<LeaveAllotmentResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("by-employee/{employeeId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<LeaveAllotmentResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByEmployee(int employeeId, [FromQuery] int? year = null)
    {
        try
        {
            var items = await _leaveAllotmentService.GetByEmployeeAsync(employeeId, year);
            return Ok(ApiResponse<IEnumerable<LeaveAllotmentResponseDto>>.SuccessResponse(items, "Leave allotments retrieved successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<IEnumerable<LeaveAllotmentResponseDto>>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<IEnumerable<LeaveAllotmentResponseDto>>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("by-leave-type/{leaveTypeId:int}/{year:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<LeaveAllotmentResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByLeaveType(int leaveTypeId, int year)
    {
        try
        {
            var items = await _leaveAllotmentService.GetByLeaveTypeAsync(leaveTypeId, year);
            return Ok(ApiResponse<IEnumerable<LeaveAllotmentResponseDto>>.SuccessResponse(items, "Leave allotments retrieved successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<IEnumerable<LeaveAllotmentResponseDto>>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<IEnumerable<LeaveAllotmentResponseDto>>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("by-year/{year:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<LeaveAllotmentResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByYear(int year)
    {
        var items = await _leaveAllotmentService.GetByYearAsync(year);
        return Ok(ApiResponse<IEnumerable<LeaveAllotmentResponseDto>>.SuccessResponse(items, "Leave allotments retrieved successfully."));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<LeaveAllotmentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateLeaveAllotmentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(BuildModelStateMessage()));
        }

        try
        {
            var updated = await _leaveAllotmentService.UpdateAsync(id, dto);
            return Ok(ApiResponse<LeaveAllotmentResponseDto>.SuccessResponse(updated, "Leave allotment updated successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<LeaveAllotmentResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<LeaveAllotmentResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<LeaveAllotmentResponseDto>.FailureResponse(ex.Message));
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
            await _leaveAllotmentService.DeleteAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null!, "Leave allotment deleted successfully."));
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
