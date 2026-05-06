using AutoMapper;
using HRM.Core.DTOs.LoanRecommendation;
using HRM.Core.Entities;

namespace HRM.API.Mappings;

public class LoanRecommendationProfile : Profile
{
    public LoanRecommendationProfile()
    {
        CreateMap<LoanRecommendation, LoanRecommendationResponseDto>()
            .ForMember(dest => dest.ApplicationNo, opt => opt.Ignore())
            .ForMember(dest => dest.EmployeeId, opt => opt.Ignore())
            .ForMember(dest => dest.EmployeeCode, opt => opt.Ignore())
            .ForMember(dest => dest.EmployeeFullName, opt => opt.Ignore())
            .ForMember(dest => dest.RequestedAmount, opt => opt.Ignore())
            .ForMember(dest => dest.RequestedAmountFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.RequestedTenureMonths, opt => opt.Ignore())
            .ForMember(dest => dest.DecisionLabel, opt => opt.Ignore())
            .ForMember(dest => dest.RecommendedAmountFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.AmountDifference, opt => opt.Ignore());

        CreateMap<CreateRecommendationDto, LoanRecommendation>();
    }
}
