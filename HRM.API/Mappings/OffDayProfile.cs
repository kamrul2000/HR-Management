using AutoMapper;
using HRM.Core.DTOs.OffDay;
using HRM.Core.Entities;

namespace HRM.API.Mappings;

public class OffDayProfile : Profile
{
    public OffDayProfile()
    {
        CreateMap<OffDay, OffDayResponseDto>()
            .ForMember(dest => dest.IsOrganizationWide, opt => opt.Ignore())
            .ForMember(dest => dest.BranchName, opt => opt.Ignore());

        CreateMap<CreateOffDayDto, OffDay>();
    }
}
