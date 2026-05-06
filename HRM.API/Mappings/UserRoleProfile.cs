using AutoMapper;
using HRM.Core.DTOs.UserRole;
using HRM.Core.Entities;

namespace HRM.API.Mappings;

public class UserRoleProfile : Profile
{
    public UserRoleProfile()
    {
        CreateMap<UserRole, UserRoleResponseDto>()
            .ForMember(dest => dest.UserName, opt => opt.Ignore())
            .ForMember(dest => dest.UserEmail, opt => opt.Ignore())
            .ForMember(dest => dest.RoleName, opt => opt.Ignore())
            .ForMember(dest => dest.AssignedAtFormatted, opt => opt.Ignore())
            .ForMember(dest => dest.RevokedAtFormatted, opt => opt.Ignore());

        CreateMap<AssignRoleDto, UserRole>();
    }
}
