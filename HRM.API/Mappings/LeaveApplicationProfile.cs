using AutoMapper;
using HRM.Core.DTOs.LeaveApplication;
using HRM.Core.Entities;

namespace HRM.API.Mappings;

public class LeaveApplicationProfile : Profile
{
    public LeaveApplicationProfile()
    {
        CreateMap<LeaveApplication, LeaveApplicationResponseDto>()
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
            .ForMember(dest => dest.FromDateFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.ToDateFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.ApprovalDateFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.StatusLabel, opt => opt.Ignore())
            .ForMember(dest => dest.AttachmentUrl, opt => opt.Ignore())
            .ForMember(dest => dest.RemainingBalanceAfter, opt => opt.Ignore());

        CreateMap<CreateLeaveApplicationDto, LeaveApplication>();
    }
}
