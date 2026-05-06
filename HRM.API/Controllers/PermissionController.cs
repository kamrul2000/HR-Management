using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.Permission;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;

[ApiController]
[Authorize]
[Route("api/permissions")]
public class PermissionController : ControllerBase
{
    private readonly IPermissionService _permissionService;

    public PermissionController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PermissionResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Upsert([FromBody] UpsertPermissionDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(BuildModelStateMessage()));
        }

        try
        {
            var result = await _permissionService.UpsertAsync(dto);
            return Ok(ApiResponse<PermissionResponseDto>.SuccessResponse(result, "Permission upserted successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<PermissionResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<PermissionResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<PermissionResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPost("bulk")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PermissionResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BulkUpsert([FromBody] BulkUpsertPermissionsDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(BuildModelStateMessage()));
        }

        try
        {
            var result = await _permissionService.BulkUpsertAsync(dto);
            return Ok(ApiResponse<IEnumerable<PermissionResponseDto>>.SuccessResponse(result, "Permissions replaced for the role successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<IEnumerable<PermissionResponseDto>>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<IEnumerable<PermissionResponseDto>>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<IEnumerable<PermissionResponseDto>>.FailureResponse(ex.Message));
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PermissionResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var items = await _permissionService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<PermissionResponseDto>>.SuccessResponse(items, "Permissions retrieved successfully."));
    }

    [HttpGet("by-role/{roleId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PermissionResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByRole(int roleId)
    {
        try
        {
            var items = await _permissionService.GetByRoleAsync(roleId);
            return Ok(ApiResponse<IEnumerable<PermissionResponseDto>>.SuccessResponse(items, "Permissions retrieved successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<IEnumerable<PermissionResponseDto>>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<IEnumerable<PermissionResponseDto>>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("my-permissions")]
    [ProducesResponseType(typeof(ApiResponse<UserPermissionSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyPermissions()
    {
        var summary = await _permissionService.GetMyPermissionsAsync();
        return Ok(ApiResponse<UserPermissionSummaryDto>.SuccessResponse(summary, "User permissions retrieved successfully."));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _permissionService.DeleteAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null!, "Permission deleted successfully."));
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
