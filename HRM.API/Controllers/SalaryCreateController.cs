using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.SalaryCreate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;

[ApiController]
[Authorize]
[Route("api/salary-structures")]
public class SalaryCreateController : ControllerBase
{
    private readonly ISalaryCreateService _salaryCreateService;

    public SalaryCreateController(ISalaryCreateService salaryCreateService)
    {
        _salaryCreateService = salaryCreateService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SalaryStructureResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateSalaryStructureDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(BuildModelStateMessage()));
        }

        try
        {
            var created = await _salaryCreateService.CreateAsync(dto);
            var location = Url.Action(nameof(GetById), new { id = created.Id }) ?? $"/api/salary-structures/{created.Id}";
            var response = ApiResponse<SalaryStructureResponseDto>.SuccessResponse(created, "Salary structure created successfully.");
            return Created(location, response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<SalaryStructureResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<SalaryStructureResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<SalaryStructureResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SalaryStructureResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var structure = await _salaryCreateService.GetByIdAsync(id);
            return Ok(ApiResponse<SalaryStructureResponseDto>.SuccessResponse(structure, "Salary structure retrieved successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<SalaryStructureResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<SalaryStructureResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("active")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SalaryStructureResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllActive()
    {
        var items = await _salaryCreateService.GetAllActiveAsync();
        return Ok(ApiResponse<IEnumerable<SalaryStructureResponseDto>>.SuccessResponse(items, "Active salary structures retrieved successfully."));
    }

    [HttpGet("employee/{employeeId:int}/active")]
    [ProducesResponseType(typeof(ApiResponse<SalaryStructureResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetActiveByEmployee(int employeeId)
    {
        try
        {
            var structure = await _salaryCreateService.GetActiveByEmployeeAsync(employeeId);
            return Ok(ApiResponse<SalaryStructureResponseDto>.SuccessResponse(structure, "Active salary structure retrieved successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<SalaryStructureResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<SalaryStructureResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("employee/{employeeId:int}/history")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SalaryStructureHistoryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHistoryByEmployee(int employeeId)
    {
        try
        {
            var items = await _salaryCreateService.GetHistoryByEmployeeAsync(employeeId);
            return Ok(ApiResponse<IEnumerable<SalaryStructureHistoryDto>>.SuccessResponse(items, "Salary structure history retrieved successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<IEnumerable<SalaryStructureHistoryDto>>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<IEnumerable<SalaryStructureHistoryDto>>.FailureResponse(ex.Message));
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SalaryStructureResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSalaryStructureDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(BuildModelStateMessage()));
        }

        try
        {
            var updated = await _salaryCreateService.UpdateAsync(id, dto);
            return Ok(ApiResponse<SalaryStructureResponseDto>.SuccessResponse(updated, "Salary structure updated successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<SalaryStructureResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<SalaryStructureResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<SalaryStructureResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPut("{id:int}/deactivate")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(int id)
    {
        try
        {
            await _salaryCreateService.DeactivateAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null!, "Salary structure deactivated successfully."));
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
