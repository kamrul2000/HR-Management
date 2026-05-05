using AutoMapper;
using HRM.Core.DTOs.Designation;
using HRM.Core.Entities;

namespace HRM.API.Mappings;

public class DesignationProfile : Profile
{
    public DesignationProfile()
    {
        CreateMap<Designation, DesignationResponseDto>()
            .ForMember(dest => dest.DepartmentName,
                       opt => opt.MapFrom(src => src.Department != null ? src.Department.Name : string.Empty))
            .ForMember(dest => dest.BranchId,
                       opt => opt.MapFrom(src => src.Department != null ? src.Department.BranchId : 0))
            .ForMember(dest => dest.BranchName,
                       opt => opt.MapFrom(src =>
                           src.Department != null && src.Department.Branch != null
                               ? src.Department.Branch.Name
                               : string.Empty))
            .ForMember(dest => dest.CompanyId,
                       opt => opt.MapFrom(src =>
                           src.Department != null && src.Department.Branch != null
                               ? src.Department.Branch.CompanyId
                               : 0))
            .ForMember(dest => dest.CompanyName,
                       opt => opt.MapFrom(src =>
                           src.Department != null && src.Department.Branch != null && src.Department.Branch.Company != null
                               ? src.Department.Branch.Company.Name
                               : string.Empty));

        CreateMap<CreateDesignationDto, Designation>();
        CreateMap<UpdateDesignationDto, Designation>();
    }
}
