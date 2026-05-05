using AutoMapper;
using HRM.Core.DTOs.Overtime;
using HRM.Core.Entities;

namespace HRM.API.Mappings;

public class OvertimeProfile : Profile
{
    public OvertimeProfile()
    {
        CreateMap<Overtime, OvertimeResponseDto>()
            .ForMember(dest => dest.EmployeeCode,
                       opt => opt.MapFrom(src => src.Employee != null ? src.Employee.EmployeeCode : string.Empty))
            .ForMember(dest => dest.EmployeeFullName,
                       opt => opt.MapFrom(src => src.Employee != null ? src.Employee.FullName : string.Empty))
            .ForMember(dest => dest.OvertimeDateFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.RequestedFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.ApprovedFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.OvertimeTypeLabel, opt => opt.Ignore())
            .ForMember(dest => dest.StatusLabel, opt => opt.Ignore())
            .ForMember(dest => dest.ApprovalDateFormatted, opt => opt.Ignore());

        CreateMap<CreateOvertimeDto, Overtime>();
    }
}
