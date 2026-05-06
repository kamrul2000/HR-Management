using HRM.Core.DTOs.LoanApproval;

namespace HRM.API.Services.Interfaces;

public interface ILoanApprovalService
{
    Task<LoanApprovalResponseDto> ProcessAsync(CreateLoanApprovalDto dto);
    Task<LoanApprovalResponseDto> GetByIdAsync(int id);
    Task<LoanApprovalResponseDto> GetByLoanApplicationAsync(int loanApplicationId);
    Task<IEnumerable<LoanApprovalResponseDto>> GetAllAsync();
}
