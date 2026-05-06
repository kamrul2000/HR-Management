using HRM.Core.DTOs.LeaveAllotment;
using HRM.Core.DTOs.PfInterest;

namespace HRM.API.Services.Interfaces;

public interface IPfInterestService
{
    // Interest rate management
    Task<PfInterestRateResponseDto> CreateRateAsync(CreatePfInterestRateDto dto);
    Task<IEnumerable<PfInterestRateResponseDto>> GetAllRatesAsync();
    Task<PfInterestRateResponseDto> GetRateByFiscalYearAsync(string fiscalYear);

    // Interest computation
    Task<EmployeePfInterestResponseDto> ComputeAsync(ComputePfInterestDto dto);
    Task<BulkCreateResultDto> BulkComputeAsync(BulkComputePfInterestDto dto);

    // Retrieval
    Task<EmployeePfInterestResponseDto> GetByIdAsync(int id);
    Task<IEnumerable<EmployeePfInterestResponseDto>> GetByEmployeeAsync(int employeeId);
    Task<PfInterestReportDto> GetReportAsync(string fiscalYear, int? branchId = null);
}
