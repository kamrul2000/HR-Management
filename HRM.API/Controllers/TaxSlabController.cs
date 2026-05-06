using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.TaxSlab;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;

[ApiController]
[Authorize]
[Route("api/tax-slabs")]
public class TaxSlabController : ControllerBase
{
    private readonly ITaxSlabService _taxSlabService;

    public TaxSlabController(ITaxSlabService taxSlabService)
    {
        _taxSlabService = taxSlabService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TaxSlabConfigResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTaxSlabConfigDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(BuildModelStateMessage()));
        }

        try
        {
            var created = await _taxSlabService.CreateAsync(dto);
            var location = Url.Action(nameof(GetById), new { id = created.Id }) ?? $"/api/tax-slabs/{created.Id}";
            var response = ApiResponse<TaxSlabConfigResponseDto>.SuccessResponse(created, "Tax slab configuration created successfully.");
            return Created(location, response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<TaxSlabConfigResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<TaxSlabConfigResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TaxSlabConfigResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var items = await _taxSlabService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<TaxSlabConfigResponseDto>>.SuccessResponse(items, "Tax slab configurations retrieved successfully."));
    }

    [HttpGet("active")]
    [ProducesResponseType(typeof(ApiResponse<TaxSlabConfigResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetActive()
    {
        try
        {
            var config = await _taxSlabService.GetActiveAsync();
            return Ok(ApiResponse<TaxSlabConfigResponseDto>.SuccessResponse(config, "Active tax slab configuration retrieved successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<TaxSlabConfigResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<TaxSlabConfigResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var config = await _taxSlabService.GetByIdAsync(id);
            return Ok(ApiResponse<TaxSlabConfigResponseDto>.SuccessResponse(config, "Tax slab configuration retrieved successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<TaxSlabConfigResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<TaxSlabConfigResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("fiscal-year/{fiscalYear}")]
    [ProducesResponseType(typeof(ApiResponse<TaxSlabConfigResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByFiscalYear(string fiscalYear)
    {
        try
        {
            var config = await _taxSlabService.GetByFiscalYearAsync(fiscalYear);
            return Ok(ApiResponse<TaxSlabConfigResponseDto>.SuccessResponse(config, "Tax slab configuration retrieved successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<TaxSlabConfigResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<TaxSlabConfigResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTaxSlabConfigDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(BuildModelStateMessage()));
        }

        try
        {
            var updated = await _taxSlabService.UpdateAsync(id, dto);
            return Ok(ApiResponse<TaxSlabConfigResponseDto>.SuccessResponse(updated, "Tax slab configuration updated successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<TaxSlabConfigResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<TaxSlabConfigResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<TaxSlabConfigResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPost("compute")]
    [ProducesResponseType(typeof(ApiResponse<TaxComputationResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Compute([FromBody] ComputeTaxDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(BuildModelStateMessage()));
        }

        try
        {
            var result = await _taxSlabService.ComputeTaxAsync(dto);
            return Ok(ApiResponse<TaxComputationResultDto>.SuccessResponse(result, "Tax computed successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<TaxComputationResultDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<TaxComputationResultDto>.FailureResponse(ex.Message));
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
            await _taxSlabService.DeleteAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null!, "Tax slab configuration deleted successfully."));
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
