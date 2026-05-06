using AutoMapper;
using HRM.Core.DTOs.EmployeeLoan;
using HRM.Core.Entities;

namespace HRM.API.Mappings;

public class EmployeeLoanProfile : Profile
{
    public EmployeeLoanProfile()
    {
        CreateMap<EmployeeLoan, EmployeeLoanResponseDto>()
            .ForMember(dest => dest.ApplicationNo, opt => opt.Ignore())
            .ForMember(dest => dest.LoanType, opt => opt.Ignore())
            .ForMember(dest => dest.EmployeeCode, opt => opt.Ignore())
            .ForMember(dest => dest.EmployeeFullName, opt => opt.Ignore())
            .ForMember(dest => dest.PrincipalAmountFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.MonthlyInstallmentFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.TotalRepayableFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.TotalPaidFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.OutstandingBalanceFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.DisbursementDateFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.TenureLabel, opt => opt.Ignore())
            .ForMember(dest => dest.InterestTypeLabel, opt => opt.Ignore())
            .ForMember(dest => dest.FirstInstallmentPeriodLabel, opt => opt.Ignore())
            .ForMember(dest => dest.RemainingInstallments, opt => opt.Ignore())
            .ForMember(dest => dest.CompletionPercentage, opt => opt.Ignore())
            .ForMember(dest => dest.StatusLabel, opt => opt.Ignore())
            .ForMember(dest => dest.Installments, opt => opt.Ignore());

        CreateMap<CreateEmployeeLoanDto, EmployeeLoan>();
    }
}
