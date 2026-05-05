using HRM.Core.DTOs.OffDay;

namespace HRM.API.Services.Interfaces;

public interface IOffDayService
{
    Task<OffDayResponseDto> CreateAsync(CreateOffDayDto dto);
    Task<IEnumerable<OffDayResponseDto>> BulkSetAsync(BulkSetOffDaysDto dto);
    Task<IEnumerable<OffDayResponseDto>> GetAllAsync(int? branchId = null);
    Task<OffDayResponseDto> GetByIdAsync(int id);
    Task<OffDayScheduleDto> GetResolvedScheduleAsync(int? branchId = null);
    Task<bool> IsOffDayAsync(DateTime date, int? branchId = null);
    Task<OffDayResponseDto> UpdateAsync(int id, UpdateOffDayDto dto);
    Task DeleteAsync(int id);
}
