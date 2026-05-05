using AutoMapper;
using HRM.Core.DTOs.LeaveAllotment;
using HRM.Core.Entities;

namespace HRM.API.Mappings;

public class LeaveAllotmentProfile : Profile
{
    public LeaveAllotmentProfile()
    {
        CreateMap<LeaveAllotment, LeaveAllotmentResponseDto>()
            .ForMember(dest => dest.EmployeeCode,
                       opt => opt.MapFrom(src => src.Employee != null ? src.Employee.EmployeeCode : string.Empty))
            .ForMember(dest => dest.EmployeeFullName,
                       opt => opt.MapFrom(src => src.Employee != null ? src.Employee.FullName : string.Empty))
            .ForMember(dest => dest.LeaveTypeName,
                       opt => opt.MapFrom(src => src.LeaveType != null ? src.LeaveType.Name : string.Empty))
            .ForMember(dest => dest.LeaveTypeCode,
                       opt => opt.MapFrom(src => src.LeaveType != null ? src.LeaveType.Code : string.Empty))
            .ForMember(dest => dest.IsPaid,
                       opt => opt.MapFrom(src => src.LeaveType != null && src.LeaveType.IsPaid))
            .ForMember(dest => dest.RemainingDays, opt => opt.Ignore());

        CreateMap<CreateLeaveAllotmentDto, LeaveAllotment>();
        CreateMap<UpdateLeaveAllotmentDto, LeaveAllotment>();
    }
}
