using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.UserRole;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;

[ApiController]
[Authorize]
[Route("api/user-roles")]
public class UserRoleController : ControllerBase
{
    private readonly IUserRoleService _userRoleService;

    public UserRoleController(IUserRoleService userRoleService)
    {
        _userRoleService = userRoleService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<UserRoleResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Assign([FromBody] AssignRoleDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(BuildModelStateMessage()));
        }

        try
        {
            var assigned = await _userRoleService.AssignAsync(dto);
            return Created($"/api/user-roles/{assigned.Id}",
                ApiResponse<UserRoleResponseDto>.SuccessResponse(assigned, "Role assigned successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<UserRoleResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<UserRoleResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<UserRoleResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPut("{id:int}/revoke")]
    [ProducesResponseType(typeof(ApiResponse<UserRoleResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Revoke(int id)
    {
        try
        {
            var revoked = await _userRoleService.RevokeAsync(id);
            return Ok(ApiResponse<UserRoleResponseDto>.SuccessResponse(revoked, "Role revoked successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<UserRoleResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<UserRoleResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<UserRoleResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("active")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserRoleResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllActive()
    {
        var items = await _userRoleService.GetAllActiveAsync();
        return Ok(ApiResponse<IEnumerable<UserRoleResponseDto>>.SuccessResponse(items, "Active user-role assignments retrieved successfully."));
    }

    [HttpGet("by-user/{userId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserRoleResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByUser(int userId)
    {
        try
        {
            var items = await _userRoleService.GetByUserAsync(userId);
            return Ok(ApiResponse<IEnumerable<UserRoleResponseDto>>.SuccessResponse(items, "User-role assignments retrieved successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<IEnumerable<UserRoleResponseDto>>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<IEnumerable<UserRoleResponseDto>>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("by-role/{roleId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserRoleResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByRole(int roleId)
    {
        try
        {
            var items = await _userRoleService.GetByRoleAsync(roleId);
            return Ok(ApiResponse<IEnumerable<UserRoleResponseDto>>.SuccessResponse(items, "User-role assignments retrieved successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<IEnumerable<UserRoleResponseDto>>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<IEnumerable<UserRoleResponseDto>>.FailureResponse(ex.Message));
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
