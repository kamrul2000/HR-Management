using AutoMapper;
using HRM.Core.DTOs.Bonus;
using HRM.Core.Entities;

namespace HRM.API.Mappings;

public class BonusProfile : Profile
{
    public BonusProfile()
    {
        CreateMap<BonusCalculation, BonusResponseDto>()
            .ForMember(dest => dest.EmployeeCode,
                       opt => opt.MapFrom(src => src.Employee != null ? src.Employee.EmployeeCode : string.Empty))
            .ForMember(dest => dest.EmployeeFullName,
                       opt => opt.MapFrom(src => src.Employee != null ? src.Employee.FullName : string.Empty))
            .ForMember(dest => dest.BonusTypeLabel, opt => opt.Ignore())
            .ForMember(dest => dest.CalculationBasisLabel, opt => opt.Ignore())
            .ForMember(dest => dest.ComputedAmountFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.FinalAmountFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.DisbursementPeriodLabel, opt => opt.Ignore())
            .ForMember(dest => dest.ApprovalDateFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.StatusLabel, opt => opt.Ignore());

        CreateMap<CreateBonusDto, BonusCalculation>();
    }
}
