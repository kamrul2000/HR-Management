using AutoMapper;
using HRM.Core.DTOs.TaxExclusion;
using HRM.Core.Entities;

namespace HRM.API.Mappings;

public class TaxExclusionProfile : Profile
{
    public TaxExclusionProfile()
    {
        CreateMap<TaxExclusion, TaxExclusionResponseDto>()
            .ForMember(dest => dest.EmployeeCode,
                       opt => opt.MapFrom(src => src.Employee != null ? src.Employee.EmployeeCode : string.Empty))
            .ForMember(dest => dest.EmployeeFullName,
                       opt => opt.MapFrom(src => src.Employee != null ? src.Employee.FullName : string.Empty))
            .ForMember(dest => dest.ExclusionTypeLabel, opt => opt.Ignore())
            .ForMember(dest => dest.PartialExclusionAmountFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.EffectiveFromFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.EffectiveToFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.IsIndefinite, opt => opt.Ignore())
            .ForMember(dest => dest.IsCurrentlyEffective, opt => opt.Ignore())
            .ForMember(dest => dest.AttachmentUrl, opt => opt.Ignore());

        CreateMap<CreateTaxExclusionDto, TaxExclusion>();
        CreateMap<UpdateTaxExclusionDto, TaxExclusion>();
    }
}
