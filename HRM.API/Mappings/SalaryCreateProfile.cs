using AutoMapper;
using HRM.Core.DTOs.SalaryCreate;
using HRM.Core.Entities;

namespace HRM.API.Mappings;

public class SalaryCreateProfile : Profile
{
    public SalaryCreateProfile()
    {
        CreateMap<SalaryStructure, SalaryStructureResponseDto>()
            .ForMember(dest => dest.EmployeeCode,
                       opt => opt.MapFrom(src => src.Employee != null ? src.Employee.EmployeeCode : string.Empty))
            .ForMember(dest => dest.EmployeeFullName,
                       opt => opt.MapFrom(src => src.Employee != null ? src.Employee.FullName : string.Empty))
            .ForMember(dest => dest.EffectiveFromFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.EffectiveToFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.BasicSalaryFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.EstimatedGrossSalary, opt => opt.Ignore())
            .ForMember(dest => dest.EstimatedDeductions, opt => opt.Ignore())
            .ForMember(dest => dest.EstimatedNetSalary, opt => opt.Ignore())
            .ForMember(dest => dest.Items, opt => opt.Ignore());

        CreateMap<CreateSalaryStructureDto, SalaryStructure>();
    }
}
