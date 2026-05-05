using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.Designation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;

[ApiController]
[Authorize]
[Route("api/designations")]
public class DesignationController : ControllerBase
{
    private readonly IDesignationService _designationService;

    public DesignationController(IDesignationService designationService)
    {
        _designationService = designationService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<DesignationResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateDesignationDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(BuildModelStateMessage()));
        }

        try
        {
            var created = await _designationService.CreateAsync(dto);
            var location = Url.Action(nameof(GetById), new { id = created.Id }) ?? $"/api/designations/{created.Id}";
            var response = ApiResponse<DesignationResponseDto>.SuccessResponse(created, "Designation created successfully.");
            return Created(location, response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<DesignationResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<DesignationResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<DesignationResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<DesignationResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var items = await _designationService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<DesignationResponseDto>>.SuccessResponse(items, "Designations retrieved successfully."));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<DesignationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var designation = await _designationService.GetByIdAsync(id);
            return Ok(ApiResponse<DesignationResponseDto>.SuccessResponse(designation, "Designation retrieved successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<DesignationResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<DesignationResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("by-department/{departmentId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<DesignationResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByDepartment(int departmentId)
    {
        try
        {
            var items = await _designationService.GetByDepartmentAsync(departmentId);
            return Ok(ApiResponse<IEnumerable<DesignationResponseDto>>.SuccessResponse(items, "Designations retrieved successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<IEnumerable<DesignationResponseDto>>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<IEnumerable<DesignationResponseDto>>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<IEnumerable<DesignationResponseDto>>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("by-branch/{branchId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<DesignationResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByBranch(int branchId)
    {
        try
        {
            var items = await _designationService.GetByBranchAsync(branchId);
            return Ok(ApiResponse<IEnumerable<DesignationResponseDto>>.SuccessResponse(items, "Designations retrieved successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<IEnumerable<DesignationResponseDto>>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<IEnumerable<DesignationResponseDto>>.FailureResponse(ex.Message));
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<DesignationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDesignationDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(BuildModelStateMessage()));
        }

        try
        {
            var updated = await _designationService.UpdateAsync(id, dto);
            return Ok(ApiResponse<DesignationResponseDto>.SuccessResponse(updated, "Designation updated successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<DesignationResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<DesignationResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<DesignationResponseDto>.FailureResponse(ex.Message));
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
            await _designationService.DeleteAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null!, "Designation deleted successfully."));
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
