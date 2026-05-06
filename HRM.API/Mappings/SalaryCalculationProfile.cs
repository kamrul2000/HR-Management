using AutoMapper;
using HRM.Core.DTOs.SalaryCalculation;
using HRM.Core.Entities;

namespace HRM.API.Mappings;

public class SalaryCalculationProfile : Profile
{
    public SalaryCalculationProfile()
    {
        CreateMap<SalaryCalculation, SalaryCalculationResponseDto>()
            .ForMember(dest => dest.EmployeeCode,
                       opt => opt.MapFrom(src => src.Employee != null ? src.Employee.EmployeeCode : string.Empty))
            .ForMember(dest => dest.EmployeeFullName,
                       opt => opt.MapFrom(src => src.Employee != null ? src.Employee.FullName : string.Empty))
            .ForMember(dest => dest.MonthLabel, opt => opt.Ignore())
            .ForMember(dest => dest.OvertimeFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.BasicSalaryFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.GrossSalaryFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.AttendanceDeduction, opt => opt.Ignore())
            .ForMember(dest => dest.AttendanceDeductionFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.TotalEarningsFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.OvertimePayFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.TotalDeductionsFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.NetSalaryFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.StatusLabel, opt => opt.Ignore())
            .ForMember(dest => dest.EarningDetails, opt => opt.Ignore())
            .ForMember(dest => dest.DeductionDetails, opt => opt.Ignore());
    }
}
