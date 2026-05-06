using AutoMapper;
using HRM.Core.DTOs.SalaryHead;
using HRM.Core.Entities;

namespace HRM.API.Mappings;

public class SalaryHeadProfile : Profile
{
    public SalaryHeadProfile()
    {
        CreateMap<SalaryHead, SalaryHeadResponseDto>()
            .ForMember(dest => dest.HeadTypeLabel, opt => opt.Ignore())
            .ForMember(dest => dest.CalculationMethodLabel, opt => opt.Ignore())
            .ForMember(dest => dest.CalculationWarning, opt => opt.Ignore())
            .ForMember(dest => dest.BaseHeadName, opt => opt.Ignore());

        CreateMap<SalaryHead, SalaryHeadSummaryDto>();

        CreateMap<CreateSalaryHeadDto, SalaryHead>();
        CreateMap<UpdateSalaryHeadDto, SalaryHead>();
    }
}
