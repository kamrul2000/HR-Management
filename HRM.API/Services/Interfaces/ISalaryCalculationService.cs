using HRM.Core.DTOs.Employee;
using HRM.Core.DTOs.LeaveAllotment;
using HRM.Core.DTOs.SalaryCalculation;

namespace HRM.API.Services.Interfaces;

public interface ISalaryCalculationService
{
    Task<SalaryCalculationResponseDto> CalculateAsync(RunSalaryCalculationDto dto);
    Task<BulkCreateResultDto> BulkCalculateAsync(BulkRunSalaryDto dto);
    Task<SalaryCalculationResponseDto> GetByIdAsync(int id);
    Task<SalaryCalculationResponseDto> GetByEmployeeMonthAsync(int employeeId, int year, int month);
    Task<PagedResultDto<SalaryCalculationResponseDto>> GetFilteredAsync(SalaryCalculationFilterDto filter);
    Task<MonthlySalaryReportDto> GetMonthlyReportAsync(int year, int month, int? branchId = null);
    Task<SalaryCalculationResponseDto> FinalizeAsync(int id);
    Task<SalaryCalculationResponseDto> CancelAsync(int id, string reason);
}
