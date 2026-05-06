using AutoMapper;
using HRM.Core.DTOs.AdditionalInfo;
using HRM.Core.Entities;

namespace HRM.API.Mappings;

public class AdditionalInfoProfile : Profile
{
    public AdditionalInfoProfile()
    {
        CreateMap<EmergencyContact, EmergencyContactDto>();
        CreateMap<CreateEmergencyContactDto, EmergencyContact>();
        CreateMap<UpdateEmergencyContactDto, EmergencyContact>();

        CreateMap<EmployeeEducation, EducationDto>()
            .ForMember(dest => dest.AttachmentUrl, opt => opt.Ignore());
        CreateMap<CreateEducationDto, EmployeeEducation>();
        CreateMap<UpdateEducationDto, EmployeeEducation>();

        CreateMap<EmployeeExperience, ExperienceDto>()
            .ForMember(dest => dest.AttachmentUrl, opt => opt.Ignore())
            .ForMember(dest => dest.FromDateFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.ToDateFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.ServiceDurationLabel, opt => opt.Ignore());
        CreateMap<CreateExperienceDto, EmployeeExperience>();
        CreateMap<UpdateExperienceDto, EmployeeExperience>();
    }
}
