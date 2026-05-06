using AutoMapper;
using HRM.Core.DTOs.LoanApplication;
using HRM.Core.Entities;

namespace HRM.API.Mappings;

public class LoanApplicationProfile : Profile
{
    public LoanApplicationProfile()
    {
        CreateMap<LoanApplication, LoanApplicationResponseDto>()
            .ForMember(dest => dest.EmployeeCode,
                       opt => opt.MapFrom(src => src.Employee != null ? src.Employee.EmployeeCode : string.Empty))
            .ForMember(dest => dest.EmployeeFullName,
                       opt => opt.MapFrom(src => src.Employee != null ? src.Employee.FullName : string.Empty))
            .ForMember(dest => dest.LoanTypeLabel, opt => opt.Ignore())
            .ForMember(dest => dest.RequestedAmountFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.TenureLabel, opt => opt.Ignore())
            .ForMember(dest => dest.EstimatedMonthlyInstallment, opt => opt.Ignore())
            .ForMember(dest => dest.EstimatedMonthlyInstallmentFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.AttachmentUrl, opt => opt.Ignore())
            .ForMember(dest => dest.StatusLabel, opt => opt.Ignore())
            .ForMember(dest => dest.RecommendationDateFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.RejectionDateFormatted, opt => opt.Ignore());

        CreateMap<CreateLoanApplicationDto, LoanApplication>();
    }
}
