using AutoMapper;
using HRM.Core.DTOs.LoanInstallment;
using HRM.Core.Entities;

namespace HRM.API.Mappings;

public class LoanInstallmentProfile : Profile
{
    public LoanInstallmentProfile()
    {
        CreateMap<LoanInstallment, LoanInstallmentResponseDto>()
            .ForMember(dest => dest.LoanNo,
                       opt => opt.MapFrom(src => src.EmployeeLoan != null ? src.EmployeeLoan.LoanNo : string.Empty))
            .ForMember(dest => dest.EmployeeCode,
                       opt => opt.MapFrom(src => src.Employee != null ? src.Employee.EmployeeCode : string.Empty))
            .ForMember(dest => dest.EmployeeFullName,
                       opt => opt.MapFrom(src => src.Employee != null ? src.Employee.FullName : string.Empty))
            .ForMember(dest => dest.DuePeriodLabel, opt => opt.Ignore())
            .ForMember(dest => dest.InstallmentAmountFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.PaidAmountFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.PaidDateFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.StatusLabel, opt => opt.Ignore());
    }
}
