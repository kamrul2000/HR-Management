using HRM.Core.DTOs.Employee;
using HRM.Core.DTOs.Overtime;

namespace HRM.API.Services.Interfaces;

public interface IOvertimeService
{
    Task<OvertimeResponseDto> CreateAsync(CreateOvertimeDto dto);
    Task<OvertimeResponseDto> GetByIdAsync(int id);
    Task<PagedResultDto<OvertimeResponseDto>> GetFilteredAsync(OvertimeFilterDto filter);
    Task<OvertimeSummaryDto> GetMonthlySummaryAsync(int employeeId, int year, int month);
    Task<IEnumerable<OvertimeSummaryDto>> GetMonthlySummaryByBranchAsync(int branchId, int year, int month);
    Task<OvertimeResponseDto> ApproveAsync(int id, ApproveOvertimeDto dto);
    Task<OvertimeResponseDto> RejectAsync(int id, RejectOvertimeDto dto);
    Task DeleteAsync(int id);
}
