using HRM.API.Services.Interfaces;

namespace HRM.API.Helpers;

public interface IWorkingDayCalculator
{
    Task<int> CountWorkingDaysAsync(DateTime from, DateTime to, int? branchId);
    Task<bool> IsWorkingDayAsync(DateTime date, int? branchId);
}

public class WorkingDayCalculator : IWorkingDayCalculator
{
    private readonly IHolidayCalendarService _holidayService;
    private readonly IOffDayService _offDayService;

    public WorkingDayCalculator(
        IHolidayCalendarService holidayService,
        IOffDayService offDayService)
    {
        _holidayService = holidayService;
        _offDayService = offDayService;
    }

    public async Task<int> CountWorkingDaysAsync(DateTime from, DateTime to, int? branchId)
    {
        if (to.Date < from.Date)
        {
            return 0;
        }

        var holidayDates = new HashSet<DateTime>();
        for (var year = from.Year; year <= to.Year; year++)
        {
            var holidays = await _holidayService.GetByYearAsync(year, branchId);
            foreach (var h in holidays)
            {
                holidayDates.Add(h.HolidayDate.Date);
            }
        }

        var schedule = await _offDayService.GetResolvedScheduleAsync(branchId);
        var offDayNumbers = schedule.OffDays.ToHashSet();

        int workingDays = 0;
        for (var date = from.Date; date <= to.Date; date = date.AddDays(1))
        {
            bool isWeeklyOff = offDayNumbers.Contains((int)date.DayOfWeek);
            bool isHoliday = holidayDates.Contains(date);
            if (!isWeeklyOff && !isHoliday)
            {
                workingDays++;
            }
        }

        return workingDays;
    }

    public async Task<bool> IsWorkingDayAsync(DateTime date, int? branchId)
    {
        var schedule = await _offDayService.GetResolvedScheduleAsync(branchId);
        if (schedule.OffDays.Contains((int)date.DayOfWeek))
        {
            return false;
        }

        var holidayCheck = await _holidayService.IsHolidayAsync(date, branchId);
        return !holidayCheck.IsHoliday;
    }
}
