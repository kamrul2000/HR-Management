using AutoMapper;
using HRM.Core.DTOs.HolidayCalendar;
using HRM.Core.Entities;

namespace HRM.API.Mappings;

public class HolidayCalendarProfile : Profile
{
    public HolidayCalendarProfile()
    {
        CreateMap<HolidayCalendar, HolidayResponseDto>()
            .ForMember(dest => dest.HolidayDateFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.HolidayTypeLabel, opt => opt.Ignore())
            .ForMember(dest => dest.IsOrganizationWide, opt => opt.Ignore())
            .ForMember(dest => dest.BranchName, opt => opt.Ignore());

        CreateMap<CreateHolidayDto, HolidayCalendar>();
        CreateMap<UpdateHolidayDto, HolidayCalendar>();
    }
}
