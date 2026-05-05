using HRM.Core.DTOs.LeaveType;

namespace HRM.API.Services.Interfaces;

public interface ILeaveTypeService
{
    Task<LeaveTypeResponseDto> CreateAsync(CreateLeaveTypeDto dto);
    Task<LeaveTypeResponseDto> GetByIdAsync(int id);
    Task<IEnumerable<LeaveTypeResponseDto>> GetAllAsync();
    Task<IEnumerable<LeaveTypeResponseDto>> GetActiveAsync();
    Task<LeaveTypeResponseDto> UpdateAsync(int id, UpdateLeaveTypeDto dto);
    Task DeleteAsync(int id);
}
