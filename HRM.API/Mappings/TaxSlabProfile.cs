using AutoMapper;
using HRM.Core.DTOs.TaxSlab;
using HRM.Core.Entities;

namespace HRM.API.Mappings;

public class TaxSlabProfile : Profile
{
    public TaxSlabProfile()
    {
        CreateMap<TaxSlabConfig, TaxSlabConfigResponseDto>()
            .ForMember(dest => dest.StartDateFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.EndDateFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.TaxFreeThresholdFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.SlabCount, opt => opt.Ignore())
            .ForMember(dest => dest.Slabs, opt => opt.Ignore());
    }
}
