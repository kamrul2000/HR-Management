using AutoMapper;
using HRM.Core.DTOs.LoanApproval;
using HRM.Core.Entities;

namespace HRM.API.Mappings;

public class LoanApprovalProfile : Profile
{
    public LoanApprovalProfile()
    {
        CreateMap<LoanApproval, LoanApprovalResponseDto>()
            .ForMember(dest => dest.ApplicationNo, opt => opt.Ignore())
            .ForMember(dest => dest.EmployeeId, opt => opt.Ignore())
            .ForMember(dest => dest.EmployeeCode, opt => opt.Ignore())
            .ForMember(dest => dest.EmployeeFullName, opt => opt.Ignore())
            .ForMember(dest => dest.RequestedAmount, opt => opt.Ignore())
            .ForMember(dest => dest.RecommendedAmount, opt => opt.Ignore())
            .ForMember(dest => dest.DecisionLabel, opt => opt.Ignore())
            .ForMember(dest => dest.ApprovedAmountFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.TenureLabel, opt => opt.Ignore())
            .ForMember(dest => dest.InterestTypeLabel, opt => opt.Ignore())
            .ForMember(dest => dest.MonthlyInstallmentFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.TotalRepayable, opt => opt.Ignore())
            .ForMember(dest => dest.TotalRepayableFormatted, opt => opt.Ignore());

        CreateMap<CreateLoanApprovalDto, LoanApproval>();
    }
}
