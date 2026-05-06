using AutoMapper;
using HRM.Core.DTOs.PfInterest;
using HRM.Core.Entities;

namespace HRM.API.Mappings;

public class PfInterestProfile : Profile
{
    public PfInterestProfile()
    {
        CreateMap<PfInterestRate, PfInterestRateResponseDto>()
            .ForMember(dest => dest.InterestRateLabel, opt => opt.Ignore())
            .ForMember(dest => dest.EffectiveFromFormatted, opt => opt.Ignore());

        CreateMap<EmployeePfInterest, EmployeePfInterestResponseDto>()
            .ForMember(dest => dest.EmployeeCode, opt => opt.Ignore())
            .ForMember(dest => dest.EmployeeFullName, opt => opt.Ignore())
            .ForMember(dest => dest.InterestRateLabel, opt => opt.Ignore())
            .ForMember(dest => dest.OpeningBalanceFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.TotalContributionsFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.AverageBalanceFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.InterestAmountFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.ClosingBalanceFormatted, opt => opt.Ignore());

        CreateMap<CreatePfInterestRateDto, PfInterestRate>();
    }
}
