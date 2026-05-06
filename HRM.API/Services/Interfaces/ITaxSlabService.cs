using HRM.Core.DTOs.TaxSlab;
using HRM.Core.Entities;

namespace HRM.API.Services.Interfaces;

public interface ITaxSlabService
{
    Task<TaxSlabConfigResponseDto> CreateAsync(CreateTaxSlabConfigDto dto);
    Task<TaxSlabConfigResponseDto> GetByIdAsync(int id);
    Task<TaxSlabConfigResponseDto> GetActiveAsync();
    Task<TaxSlabConfigResponseDto> GetByFiscalYearAsync(string fiscalYear);
    Task<IEnumerable<TaxSlabConfigResponseDto>> GetAllAsync();
    Task<TaxSlabConfigResponseDto> UpdateAsync(int id, UpdateTaxSlabConfigDto dto);
    Task<TaxComputationResultDto> ComputeTaxAsync(ComputeTaxDto dto);
    Task DeleteAsync(int id);

    /// <summary>
    /// Internal cross-service accessor for the active tax config of a tenant.
    /// Bypasses HttpContext — used by Module 17 (SalaryCalculation).
    /// Returns null when no active config exists.
    /// </summary>
    Task<TaxSlabConfig?> GetActiveConfigAsync(int subscriptionId);
}
