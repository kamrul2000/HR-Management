using AutoMapper;
using HRM.Core.DTOs.DutySlot;
using HRM.Core.Entities;

namespace HRM.API.Mappings;

public class DutySlotProfile : Profile
{
    public DutySlotProfile()
    {
        CreateMap<DutySlot, DutySlotResponseDto>()
            .ForMember(dest => dest.StartTimeFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.EndTimeFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.TotalWorkingHoursFormatted, opt => opt.Ignore());

        CreateMap<CreateDutySlotDto, DutySlot>();
        CreateMap<UpdateDutySlotDto, DutySlot>();
    }
}
