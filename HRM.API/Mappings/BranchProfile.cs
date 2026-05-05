using AutoMapper;
using HRM.Core.DTOs.Branch;
using HRM.Core.Entities;

namespace HRM.API.Mappings;

public class BranchProfile : Profile
{
    public BranchProfile()
    {
        CreateMap<Branch, BranchResponseDto>()
            .ForMember(dest => dest.CompanyName,
                       opt => opt.MapFrom(src => src.Company != null ? src.Company.Name : string.Empty));

        CreateMap<CreateBranchDto, Branch>();
        CreateMap<UpdateBranchDto, Branch>();
    }
}
