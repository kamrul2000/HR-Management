using AutoMapper;
using HRM.Core.DTOs.GratuityCalculation;
using HRM.Core.Entities;

namespace HRM.API.Mappings;

public class GratuityCalculationProfile : Profile
{
    public GratuityCalculationProfile()
    {
        CreateMap<GratuityCalculation, GratuityCalculationResponseDto>()
            .ForMember(dest => dest.EmployeeCode, opt => opt.Ignore())
            .ForMember(dest => dest.EmployeeFullName, opt => opt.Ignore())
            .ForMember(dest => dest.RuleName, opt => opt.Ignore())
            .ForMember(dest => dest.SeparationDateFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.JoiningDateFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.ServicePeriodLabel, opt => opt.Ignore())
            .ForMember(dest => dest.CalculationBasisLabel, opt => opt.Ignore())
            .ForMember(dest => dest.MonthlySalaryFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.DailySalaryFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.GratuityBeforeCapFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.GratuityAmountFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.StatusLabel, opt => opt.Ignore());

        CreateMap<ComputeGratuityDto, GratuityCalculation>();
    }
}
