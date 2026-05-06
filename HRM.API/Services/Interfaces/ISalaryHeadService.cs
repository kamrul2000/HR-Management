using HRM.Core.DTOs.SalaryHead;

namespace HRM.API.Services.Interfaces;

public interface ISalaryHeadService
{
    Task<SalaryHeadResponseDto> CreateAsync(CreateSalaryHeadDto dto);
    Task<SalaryHeadResponseDto> GetByIdAsync(int id);
    Task<IEnumerable<SalaryHeadResponseDto>> GetAllAsync();
    Task<IEnumerable<SalaryHeadResponseDto>> GetEarningsAsync();
    Task<IEnumerable<SalaryHeadResponseDto>> GetDeductionsAsync();
    Task<IEnumerable<SalaryHeadSummaryDto>> GetActiveHeadsSummaryAsync();
    Task<SalaryHeadResponseDto> UpdateAsync(int id, UpdateSalaryHeadDto dto);
    Task DeleteAsync(int id);
}
