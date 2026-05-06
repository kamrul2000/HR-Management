using HRM.Core.DTOs.Employee;
using HRM.Core.DTOs.LoanApplication;

namespace HRM.API.Services.Interfaces;

public interface ILoanApplicationService
{
    Task<LoanApplicationResponseDto> CreateAsync(CreateLoanApplicationDto dto);
    Task<LoanApplicationResponseDto> UploadAttachmentAsync(int id, IFormFile file);
    Task<LoanApplicationResponseDto> GetByIdAsync(int id);
    Task<PagedResultDto<LoanApplicationResponseDto>> GetFilteredAsync(LoanApplicationFilterDto filter);
    Task<IEnumerable<LoanApplicationResponseDto>> GetByEmployeeAsync(int employeeId);
    Task<LoanApplicationResponseDto> CancelAsync(int id, CancelLoanApplicationDto dto);
    Task DeleteAsync(int id);
}
