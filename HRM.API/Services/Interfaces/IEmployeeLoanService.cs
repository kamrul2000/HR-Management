using HRM.Core.DTOs.Employee;
using HRM.Core.DTOs.EmployeeLoan;

namespace HRM.API.Services.Interfaces;

public interface IEmployeeLoanService
{
    Task<EmployeeLoanResponseDto> CreateAsync(CreateEmployeeLoanDto dto);
    Task<EmployeeLoanResponseDto> GetByIdAsync(int id);
    Task<EmployeeLoanResponseDto> GetByEmployeeAsync(int employeeId);
    Task<PagedResultDto<EmployeeLoanResponseDto>> GetFilteredAsync(EmployeeLoanFilterDto filter);
    Task<EmployeeLoanResponseDto> MarkCompletedAsync(int id);
    Task<EmployeeLoanResponseDto> MarkDefaultedAsync(int id, string reason);
    Task<EmployeeLoanResponseDto> CancelAsync(int id, string reason);
}
