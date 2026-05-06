using HRM.Core.DTOs.Bonus;
using HRM.Core.DTOs.Employee;
using HRM.Core.DTOs.LeaveAllotment;

namespace HRM.API.Services.Interfaces;

public interface IBonusCalculationService
{
    Task<BonusResponseDto> CreateAsync(CreateBonusDto dto);
    Task<BulkCreateResultDto> BulkCreateAsync(BulkCreateBonusDto dto);
    Task<BonusResponseDto> GetByIdAsync(int id);
    Task<PagedResultDto<BonusResponseDto>> GetFilteredAsync(BonusFilterDto filter);
    Task<IEnumerable<BonusResponseDto>> GetByEmployeeAsync(int employeeId);
    Task<BonusSummaryReportDto> GetSummaryReportAsync(int year, int month, int? branchId = null);
    Task<BonusResponseDto> ApproveAsync(int id, ApproveBonusDto dto);
    Task<BonusResponseDto> DisburseAsync(int id, DisburseBonusDto dto);
    Task<BonusResponseDto> CancelAsync(int id, string reason);
    Task DeleteAsync(int id);
}
