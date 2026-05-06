using AutoMapper;
using HRM.Core.DTOs.SeparationReason;
using HRM.Core.Entities;

namespace HRM.API.Mappings;

public class SeparationReasonProfile : Profile
{
    public SeparationReasonProfile()
    {
        CreateMap<SeparationReason, SeparationReasonResponseDto>()
            .ForMember(dest => dest.SeparationTypeLabel, opt => opt.Ignore());

        CreateMap<CreateSeparationReasonDto, SeparationReason>();
        CreateMap<UpdateSeparationReasonDto, SeparationReason>();
    }
}
