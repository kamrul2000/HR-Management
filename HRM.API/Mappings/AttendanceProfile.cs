using AutoMapper;
using HRM.Core.DTOs.Attendance;
using HRM.Core.Entities;

namespace HRM.API.Mappings;

public class AttendanceProfile : Profile
{
    public AttendanceProfile()
    {
        CreateMap<Attendance, AttendanceResponseDto>()
            .ForMember(dest => dest.EmployeeCode,
                       opt => opt.MapFrom(src => src.Employee != null ? src.Employee.EmployeeCode : string.Empty))
            .ForMember(dest => dest.EmployeeFullName,
                       opt => opt.MapFrom(src => src.Employee != null ? src.Employee.FullName : string.Empty))
            .ForMember(dest => dest.SlotName,
                       opt => opt.MapFrom(src => src.DutySlot != null ? src.DutySlot.SlotName : string.Empty))
            .ForMember(dest => dest.ShiftTime, opt => opt.Ignore())
            .ForMember(dest => dest.AttendanceDateFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.PunchInFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.PunchOutFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.StatusLabel, opt => opt.Ignore())
            .ForMember(dest => dest.LateFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.ActualWorkingHoursFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.OvertimeFormatted, opt => opt.Ignore());

        CreateMap<CreateAttendanceDto, Attendance>();
        CreateMap<UpdateAttendanceDto, Attendance>();
    }
}
