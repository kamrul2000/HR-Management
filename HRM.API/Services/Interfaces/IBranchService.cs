using HRM.Core.DTOs.Branch;

namespace HRM.API.Services.Interfaces;

public interface IBranchService
{
    Task<BranchResponseDto> CreateAsync(CreateBranchDto dto);
    Task<BranchResponseDto> GetByIdAsync(int id);
    Task<IEnumerable<BranchResponseDto>> GetAllAsync();
    Task<IEnumerable<BranchResponseDto>> GetByCompanyAsync(int companyId);
    Task<BranchResponseDto> UpdateAsync(int id, UpdateBranchDto dto);
    Task DeleteAsync(int id);
}
