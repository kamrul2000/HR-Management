using HRM.Core.DTOs.SalaryCreate;
using HRM.Core.Entities;

namespace HRM.API.Services.Interfaces;

public interface ISalaryCreateService
{
    Task<SalaryStructureResponseDto> CreateAsync(CreateSalaryStructureDto dto);
    Task<SalaryStructureResponseDto> GetByIdAsync(int id);
    Task<SalaryStructureResponseDto> GetActiveByEmployeeAsync(int employeeId);
    Task<IEnumerable<SalaryStructureHistoryDto>> GetHistoryByEmployeeAsync(int employeeId);
    Task<IEnumerable<SalaryStructureResponseDto>> GetAllActiveAsync();
    Task<SalaryStructureResponseDto> UpdateAsync(int id, UpdateSalaryStructureDto dto);
    Task DeactivateAsync(int id);

    /// <summary>
    /// Returns the salary structure that was active on the given date for the
    /// given employee, including its Items and SalaryHead navigation.
    /// Used by Module 17 (SalaryCalculation) to honor mid-month revisions —
    /// the structure active on the 1st of the month is the one applied.
    /// </summary>
    Task<SalaryStructure?> GetStructureActiveOnDateAsync(int employeeId, DateTime date);

    /// <summary>
    /// Returns the currently active salary structure DTO for the given employee
    /// using a caller-supplied subscriptionId — bypasses IHttpContextAccessor.
    /// Used by Module 26 (PfContribution) when computing PF inside other services.
    /// Returns null when no active structure exists; never throws.
    /// </summary>
    Task<SalaryStructureResponseDto?> GetActiveByEmployeeInternalAsync(int employeeId, int subscriptionId);
}
