using HRM.Core.DTOs.Employee;
using HRM.Core.DTOs.LeaveAllotment;
using HRM.Core.DTOs.PfContribution;

namespace HRM.API.Services.Interfaces;

public interface IPfContributionService
{
    // PF Rule management
    Task<PfRuleResponseDto> CreateRuleAsync(CreatePfRuleDto dto);
    Task<PfRuleResponseDto> GetRuleByIdAsync(int id);
    Task<PfRuleResponseDto> GetActiveRuleAsync();
    Task<IEnumerable<PfRuleResponseDto>> GetAllRulesAsync();
    Task<PfRuleResponseDto> UpdateRuleAsync(int id, UpdatePfRuleDto dto);

    // PF Contribution computation and retrieval
    Task<EmployeePfContributionResponseDto> ComputeAsync(int employeeId, int year, int month);
    Task<BulkCreateResultDto> BulkComputeAsync(int year, int month, int? branchId = null);
    Task<EmployeePfContributionResponseDto> GetContributionByIdAsync(int id);
    Task<IEnumerable<EmployeePfContributionResponseDto>> GetByEmployeeAsync(int employeeId, int? year = null);
    Task<PagedResultDto<EmployeePfContributionResponseDto>> GetFilteredAsync(PfContributionFilterDto filter);
    Task<PfMonthlyReportDto> GetMonthlyReportAsync(int year, int month, int? branchId = null);

    // Internal — used by Module 17 (SalaryCalculation)
    Task<decimal> GetEmployeeMonthlyPfAsync(int employeeId, int subscriptionId);
}
