using AutoMapper;
using HRM.Core.DTOs.Employee;
using HRM.Core.Entities;

namespace HRM.API.Mappings;

public class EmployeeProfile : Profile
{
    public EmployeeProfile()
    {
        CreateMap<Employee, EmployeeResponseDto>()
            .ForMember(dest => dest.BranchName,
                       opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
            .ForMember(dest => dest.CompanyId,
                       opt => opt.MapFrom(src => src.Branch != null ? src.Branch.CompanyId : 0))
            .ForMember(dest => dest.CompanyName,
                       opt => opt.MapFrom(src =>
                           src.Branch != null && src.Branch.Company != null
                               ? src.Branch.Company.Name
                               : string.Empty))
            .ForMember(dest => dest.DepartmentName,
                       opt => opt.MapFrom(src => src.Department != null ? src.Department.Name : string.Empty))
            .ForMember(dest => dest.DesignationTitle,
                       opt => opt.MapFrom(src => src.Designation != null ? src.Designation.Title : string.Empty))
            .ForMember(dest => dest.PhotoUrl, opt => opt.Ignore());

        CreateMap<Employee, EmployeeListDto>()
            .ForMember(dest => dest.BranchName,
                       opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
            .ForMember(dest => dest.DepartmentName,
                       opt => opt.MapFrom(src => src.Department != null ? src.Department.Name : string.Empty))
            .ForMember(dest => dest.DesignationTitle,
                       opt => opt.MapFrom(src => src.Designation != null ? src.Designation.Title : string.Empty))
            .ForMember(dest => dest.PhotoUrl, opt => opt.Ignore());

        CreateMap<CreateEmployeeDto, Employee>();
        CreateMap<UpdateEmployeeDto, Employee>();
    }
}
