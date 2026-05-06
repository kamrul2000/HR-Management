using AutoMapper;
using HRM.Core.DTOs.GratuitySetup;
using HRM.Core.Entities;

namespace HRM.API.Mappings;

public class GratuityRuleProfile : Profile
{
    public GratuityRuleProfile()
    {
        CreateMap<GratuityRule, GratuityRuleResponseDto>()
            .ForMember(dest => dest.MinServiceYearsLabel, opt => opt.Ignore())
            .ForMember(dest => dest.CalculationBasisLabel, opt => opt.Ignore())
            .ForMember(dest => dest.RatePerYearLabel, opt => opt.Ignore())
            .ForMember(dest => dest.MaxGratuityAmountFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.MaxServiceYearsCappedLabel, opt => opt.Ignore())
            .ForMember(dest => dest.ProRataLabel, opt => opt.Ignore())
            .ForMember(dest => dest.EffectiveFromFormatted, opt => opt.Ignore());

        CreateMap<CreateGratuityRuleDto, GratuityRule>();
        CreateMap<UpdateGratuityRuleDto, GratuityRule>();
    }
}
