using HRM.Core.DTOs.Employee;
using HRM.Core.DTOs.LoanInstallment;

namespace HRM.API.Services.Interfaces;

public interface ILoanInstallmentService
{
    Task<LoanInstallmentResponseDto> GetByIdAsync(int id);
    Task<PagedResultDto<LoanInstallmentResponseDto>> GetFilteredAsync(InstallmentFilterDto filter);
    Task<IEnumerable<LoanInstallmentResponseDto>> GetByLoanAsync(int employeeLoanId);
    Task<PendingInstallmentDto?> GetPendingInstallmentAsync(int employeeId, int year, int month);
    Task<LoanInstallmentResponseDto> ProcessPaymentAsync(int id, ProcessInstallmentDto dto);
    Task<LoanInstallmentResponseDto> SkipAsync(int id, SkipInstallmentDto dto);
    Task<LoanInstallmentResponseDto> ReinstateAsync(int id);
    Task<int> MarkOverdueAsync(int year, int month);
}
