using HRM.Core.DTOs.AdditionalInfo;

namespace HRM.API.Services.Interfaces;

public interface IAdditionalInfoService
{
    // Emergency Contacts
    Task<EmergencyContactDto> AddEmergencyContactAsync(int employeeId, CreateEmergencyContactDto dto);
    Task<IEnumerable<EmergencyContactDto>> GetEmergencyContactsAsync(int employeeId);
    Task<EmergencyContactDto> UpdateEmergencyContactAsync(int contactId, UpdateEmergencyContactDto dto);
    Task DeleteEmergencyContactAsync(int contactId);
    Task<EmergencyContactDto> SetPrimaryContactAsync(int contactId);

    // Education
    Task<EducationDto> AddEducationAsync(int employeeId, CreateEducationDto dto);
    Task<IEnumerable<EducationDto>> GetEducationAsync(int employeeId);
    Task<EducationDto> UpdateEducationAsync(int educationId, UpdateEducationDto dto);
    Task<EducationDto> UploadEducationAttachmentAsync(int educationId, IFormFile file);
    Task DeleteEducationAsync(int educationId);

    // Experience
    Task<ExperienceDto> AddExperienceAsync(int employeeId, CreateExperienceDto dto);
    Task<IEnumerable<ExperienceDto>> GetExperienceAsync(int employeeId);
    Task<ExperienceDto> UpdateExperienceAsync(int experienceId, UpdateExperienceDto dto);
    Task<ExperienceDto> UploadExperienceAttachmentAsync(int experienceId, IFormFile file);
    Task DeleteExperienceAsync(int experienceId);
}
