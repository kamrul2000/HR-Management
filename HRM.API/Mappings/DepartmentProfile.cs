using AutoMapper;
using HRM.Core.DTOs.Department;
using HRM.Core.Entities;

namespace HRM.API.Mappings;

public class DepartmentProfile : Profile
{
    public DepartmentProfile()
    {
        CreateMap<Department, DepartmentResponseDto>()
            .ForMember(dest => dest.BranchName,
                       opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
            .ForMember(dest => dest.CompanyId,
                       opt => opt.MapFrom(src => src.Branch != null ? src.Branch.CompanyId : 0))
            .ForMember(dest => dest.CompanyName,
                       opt => opt.MapFrom(src =>
                           src.Branch != null && src.Branch.Company != null
                               ? src.Branch.Company.Name
                               : string.Empty));

        CreateMap<CreateDepartmentDto, Department>();
        CreateMap<UpdateDepartmentDto, Department>();
    }
}
