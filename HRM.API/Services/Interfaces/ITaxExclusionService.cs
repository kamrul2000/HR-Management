using HRM.Core.DTOs.TaxExclusion;

namespace HRM.API.Services.Interfaces;

public interface ITaxExclusionService
{
    Task<TaxExclusionResponseDto> CreateAsync(CreateTaxExclusionDto dto);
    Task<TaxExclusionResponseDto> UploadAttachmentAsync(int id, IFormFile file);
    Task<TaxExclusionResponseDto> GetByIdAsync(int id);
    Task<IEnumerable<TaxExclusionResponseDto>> GetByEmployeeAsync(int employeeId);
    Task<IEnumerable<TaxExclusionResponseDto>> GetAllActiveAsync();
    Task<TaxExclusionCheckDto> CheckExclusionAsync(int employeeId);

    /// <summary>
    /// Internal cross-service overload bypassing HttpContext.
    /// Used by Module 17 (SalaryCalculation).
    /// </summary>
    Task<TaxExclusionCheckDto> CheckExclusionAsync(int employeeId, int subscriptionId);

    Task<TaxExclusionResponseDto> UpdateAsync(int id, UpdateTaxExclusionDto dto);
    Task DeleteAsync(int id);
}
