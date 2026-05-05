using HRM.Core.DTOs.HolidayCalendar;
using HRM.Core.DTOs.LeaveAllotment;

namespace HRM.API.Services.Interfaces;

public interface IHolidayCalendarService
{
    Task<HolidayResponseDto> CreateAsync(CreateHolidayDto dto);
    Task<BulkCreateResultDto> BulkCreateAsync(BulkCreateHolidayDto dto);
    Task<HolidayResponseDto> GetByIdAsync(int id);
    Task<IEnumerable<HolidayResponseDto>> GetFilteredAsync(HolidayFilterDto filter);
    Task<IEnumerable<HolidayResponseDto>> GetByYearAsync(int year, int? branchId = null);
    Task<HolidayCheckResultDto> IsHolidayAsync(DateTime date, int? branchId = null);
    Task<HolidayResponseDto> UpdateAsync(int id, UpdateHolidayDto dto);
    Task DeleteAsync(int id);
}
