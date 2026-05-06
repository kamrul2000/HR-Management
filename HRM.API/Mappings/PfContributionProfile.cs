using AutoMapper;
using HRM.Core.DTOs.PfContribution;
using HRM.Core.Entities;

namespace HRM.API.Mappings;

public class PfContributionProfile : Profile
{
    public PfContributionProfile()
    {
        CreateMap<PfRule, PfRuleResponseDto>()
            .ForMember(dest => dest.EmployeeContributionRateLabel,
                       opt => opt.MapFrom(src => $"{src.EmployeeContributionRate:F2}%"))
            .ForMember(dest => dest.EmployerContributionRateLabel,
                       opt => opt.MapFrom(src => $"{src.EmployerContributionRate:F2}%"))
            .ForMember(dest => dest.PfBaseLabel,
                       opt => opt.MapFrom(src =>
                           src.PfBase == "Basic" ? "Basic Salary" : "PF-Applicable Heads"))
            .ForMember(dest => dest.MinEligibleSalaryFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.MaxContributionAmountFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.EffectiveFromFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.EffectiveToFormatted, opt => opt.Ignore());

        CreateMap<EmployeePfContribution, EmployeePfContributionResponseDto>()
            .ForMember(dest => dest.EmployeeCode, opt => opt.Ignore())
            .ForMember(dest => dest.EmployeeFullName, opt => opt.Ignore())
            .ForMember(dest => dest.RuleName, opt => opt.Ignore())
            .ForMember(dest => dest.PeriodLabel, opt => opt.Ignore())
            .ForMember(dest => dest.PfBaseFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.EmployeeContributionFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.EmployerContributionFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.TotalContributionFormatted, opt => opt.Ignore());

        CreateMap<CreatePfRuleDto, PfRule>();
        CreateMap<UpdatePfRuleDto, PfRule>();
    }
}
