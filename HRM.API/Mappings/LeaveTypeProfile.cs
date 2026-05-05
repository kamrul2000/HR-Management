using AutoMapper;
using HRM.Core.DTOs.LeaveType;
using HRM.Core.Entities;

namespace HRM.API.Mappings;

public class LeaveTypeProfile : Profile
{
    public LeaveTypeProfile()
    {
        CreateMap<LeaveType, LeaveTypeResponseDto>()
            .ForMember(dest => dest.IsPaidLabel, opt => opt.Ignore())
            .ForMember(dest => dest.GenderRestrictionLabel, opt => opt.Ignore());

        CreateMap<CreateLeaveTypeDto, LeaveType>();
        CreateMap<UpdateLeaveTypeDto, LeaveType>();
    }
}
