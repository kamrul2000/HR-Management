using AutoMapper;
using HRM.Core.DTOs.EmployeeSeparation;
using HRM.Core.Entities;

namespace HRM.API.Mappings;

public class EmployeeSeparationProfile : Profile
{
    public EmployeeSeparationProfile()
    {
        CreateMap<EmployeeSeparation, SeparationResponseDto>()
            .ForMember(dest => dest.EmployeeCode, opt => opt.Ignore())
            .ForMember(dest => dest.EmployeeFullName, opt => opt.Ignore())
            .ForMember(dest => dest.EmployeeJoiningDate, opt => opt.Ignore())
            .ForMember(dest => dest.EmployeeJoiningDateFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.SeparationReasonName, opt => opt.Ignore())
            .ForMember(dest => dest.SeparationTypeLabel, opt => opt.Ignore())
            .ForMember(dest => dest.ApplicationDateFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.LastWorkingDateFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.NoticePeriodShortfallLabel, opt => opt.Ignore())
            .ForMember(dest => dest.NoticePeriodBuyoutFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.GratuityAmountFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.OtherSettlementAmountFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.TotalSettlementAmountFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.AttachmentUrl, opt => opt.Ignore())
            .ForMember(dest => dest.StatusLabel, opt => opt.Ignore())
            .ForMember(dest => dest.ApprovalDateFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.ProcessedDateFormatted, opt => opt.Ignore());

        CreateMap<CreateSeparationDto, EmployeeSeparation>();
    }
}
