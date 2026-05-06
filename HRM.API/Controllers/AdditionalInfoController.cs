using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.AdditionalInfo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;

[ApiController]
[Authorize]
[Route("api/additional-info")]
public class AdditionalInfoController : ControllerBase
{
    private readonly IAdditionalInfoService _service;

    public AdditionalInfoController(IAdditionalInfoService service)
    {
        _service = service;
    }

    // ───────────────────── Emergency Contacts

    [HttpPost("{employeeId:int}/emergency-contacts")]
    [ProducesResponseType(typeof(ApiResponse<EmergencyContactDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddEmergencyContact(int employeeId, [FromBody] CreateEmergencyContactDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(BuildModelStateMessage()));
        }

        try
        {
            var created = await _service.AddEmergencyContactAsync(employeeId, dto);
            return Created($"/api/additional-info/emergency-contacts/{created.Id}",
                ApiResponse<EmergencyContactDto>.SuccessResponse(created, "Emergency contact added successfully."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<EmergencyContactDto>.FailureResponse(ex.Message)); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse<EmergencyContactDto>.FailureResponse(ex.Message)); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<EmergencyContactDto>.FailureResponse(ex.Message)); }
    }

    [HttpGet("{employeeId:int}/emergency-contacts")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<EmergencyContactDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmergencyContacts(int employeeId)
    {
        try
        {
            var items = await _service.GetEmergencyContactsAsync(employeeId);
            return Ok(ApiResponse<IEnumerable<EmergencyContactDto>>.SuccessResponse(items, "Emergency contacts retrieved successfully."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<IEnumerable<EmergencyContactDto>>.FailureResponse(ex.Message)); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse<IEnumerable<EmergencyContactDto>>.FailureResponse(ex.Message)); }
    }

    [HttpPut("emergency-contacts/{contactId:int}")]
    [ProducesResponseType(typeof(ApiResponse<EmergencyContactDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateEmergencyContact(int contactId, [FromBody] UpdateEmergencyContactDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(BuildModelStateMessage()));
        }

        try
        {
            var updated = await _service.UpdateEmergencyContactAsync(contactId, dto);
            return Ok(ApiResponse<EmergencyContactDto>.SuccessResponse(updated, "Emergency contact updated successfully."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<EmergencyContactDto>.FailureResponse(ex.Message)); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse<EmergencyContactDto>.FailureResponse(ex.Message)); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<EmergencyContactDto>.FailureResponse(ex.Message)); }
    }

    [HttpPut("emergency-contacts/{contactId:int}/set-primary")]
    [ProducesResponseType(typeof(ApiResponse<EmergencyContactDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SetPrimary(int contactId)
    {
        try
        {
            var updated = await _service.SetPrimaryContactAsync(contactId);
            return Ok(ApiResponse<EmergencyContactDto>.SuccessResponse(updated, "Primary emergency contact updated successfully."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<EmergencyContactDto>.FailureResponse(ex.Message)); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse<EmergencyContactDto>.FailureResponse(ex.Message)); }
    }

    [HttpDelete("emergency-contacts/{contactId:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteEmergencyContact(int contactId)
    {
        try
        {
            await _service.DeleteEmergencyContactAsync(contactId);
            return Ok(ApiResponse<object>.SuccessResponse(null!, "Emergency contact deleted successfully."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<object>.FailureResponse(ex.Message)); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse<object>.FailureResponse(ex.Message)); }
    }

    // ───────────────────── Education

    [HttpPost("{employeeId:int}/education")]
    [ProducesResponseType(typeof(ApiResponse<EducationDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddEducation(int employeeId, [FromBody] CreateEducationDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(BuildModelStateMessage()));
        }

        try
        {
            var created = await _service.AddEducationAsync(employeeId, dto);
            return Created($"/api/additional-info/education/{created.Id}",
                ApiResponse<EducationDto>.SuccessResponse(created, "Education record added successfully."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<EducationDto>.FailureResponse(ex.Message)); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse<EducationDto>.FailureResponse(ex.Message)); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<EducationDto>.FailureResponse(ex.Message)); }
    }

    [HttpGet("{employeeId:int}/education")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<EducationDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEducation(int employeeId)
    {
        try
        {
            var items = await _service.GetEducationAsync(employeeId);
            return Ok(ApiResponse<IEnumerable<EducationDto>>.SuccessResponse(items, "Education records retrieved successfully."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<IEnumerable<EducationDto>>.FailureResponse(ex.Message)); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse<IEnumerable<EducationDto>>.FailureResponse(ex.Message)); }
    }

    [HttpPut("education/{educationId:int}")]
    [ProducesResponseType(typeof(ApiResponse<EducationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateEducation(int educationId, [FromBody] UpdateEducationDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(BuildModelStateMessage()));
        }

        try
        {
            var updated = await _service.UpdateEducationAsync(educationId, dto);
            return Ok(ApiResponse<EducationDto>.SuccessResponse(updated, "Education record updated successfully."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<EducationDto>.FailureResponse(ex.Message)); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse<EducationDto>.FailureResponse(ex.Message)); }
    }

    [HttpPost("education/{educationId:int}/attachment")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    [ProducesResponseType(typeof(ApiResponse<EducationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UploadEducationAttachment(int educationId, IFormFile file)
    {
        try
        {
            var updated = await _service.UploadEducationAttachmentAsync(educationId, file);
            return Ok(ApiResponse<EducationDto>.SuccessResponse(updated, "Education certificate uploaded successfully."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<EducationDto>.FailureResponse(ex.Message)); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse<EducationDto>.FailureResponse(ex.Message)); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<EducationDto>.FailureResponse(ex.Message)); }
    }

    [HttpDelete("education/{educationId:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteEducation(int educationId)
    {
        try
        {
            await _service.DeleteEducationAsync(educationId);
            return Ok(ApiResponse<object>.SuccessResponse(null!, "Education record deleted successfully."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<object>.FailureResponse(ex.Message)); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse<object>.FailureResponse(ex.Message)); }
    }

    // ───────────────────── Experience

    [HttpPost("{employeeId:int}/experience")]
    [ProducesResponseType(typeof(ApiResponse<ExperienceDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddExperience(int employeeId, [FromBody] CreateExperienceDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(BuildModelStateMessage()));
        }

        try
        {
            var created = await _service.AddExperienceAsync(employeeId, dto);
            return Created($"/api/additional-info/experience/{created.Id}",
                ApiResponse<ExperienceDto>.SuccessResponse(created, "Experience record added successfully."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<ExperienceDto>.FailureResponse(ex.Message)); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse<ExperienceDto>.FailureResponse(ex.Message)); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<ExperienceDto>.FailureResponse(ex.Message)); }
    }

    [HttpGet("{employeeId:int}/experience")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ExperienceDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExperience(int employeeId)
    {
        try
        {
            var items = await _service.GetExperienceAsync(employeeId);
            return Ok(ApiResponse<IEnumerable<ExperienceDto>>.SuccessResponse(items, "Experience records retrieved successfully."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<IEnumerable<ExperienceDto>>.FailureResponse(ex.Message)); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse<IEnumerable<ExperienceDto>>.FailureResponse(ex.Message)); }
    }

    [HttpPut("experience/{experienceId:int}")]
    [ProducesResponseType(typeof(ApiResponse<ExperienceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateExperience(int experienceId, [FromBody] UpdateExperienceDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(BuildModelStateMessage()));
        }

        try
        {
            var updated = await _service.UpdateExperienceAsync(experienceId, dto);
            return Ok(ApiResponse<ExperienceDto>.SuccessResponse(updated, "Experience record updated successfully."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<ExperienceDto>.FailureResponse(ex.Message)); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse<ExperienceDto>.FailureResponse(ex.Message)); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<ExperienceDto>.FailureResponse(ex.Message)); }
    }

    [HttpPost("experience/{experienceId:int}/attachment")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    [ProducesResponseType(typeof(ApiResponse<ExperienceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UploadExperienceAttachment(int experienceId, IFormFile file)
    {
        try
        {
            var updated = await _service.UploadExperienceAttachmentAsync(experienceId, file);
            return Ok(ApiResponse<ExperienceDto>.SuccessResponse(updated, "Experience letter uploaded successfully."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<ExperienceDto>.FailureResponse(ex.Message)); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse<ExperienceDto>.FailureResponse(ex.Message)); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<ExperienceDto>.FailureResponse(ex.Message)); }
    }

    [HttpDelete("experience/{experienceId:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteExperience(int experienceId)
    {
        try
        {
            await _service.DeleteExperienceAsync(experienceId);
            return Ok(ApiResponse<object>.SuccessResponse(null!, "Experience record deleted successfully."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<object>.FailureResponse(ex.Message)); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse<object>.FailureResponse(ex.Message)); }
    }

    private string BuildModelStateMessage()
    {
        return string.Join(" ",
            ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
    }
}
