using HRM.Core.DTOs.DutySlot;

namespace HRM.API.Services.Interfaces;

public interface IDutySlotService
{
    Task<DutySlotResponseDto> CreateAsync(CreateDutySlotDto dto);
    Task<DutySlotResponseDto> GetByIdAsync(int id);
    Task<IEnumerable<DutySlotResponseDto>> GetAllAsync();
    Task<DutySlotResponseDto> UpdateAsync(int id, UpdateDutySlotDto dto);
    Task DeleteAsync(int id);
}
