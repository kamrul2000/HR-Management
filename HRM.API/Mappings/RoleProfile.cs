using AutoMapper;
using HRM.Core.DTOs.Role;
using HRM.Core.Entities;

namespace HRM.API.Mappings;

public class RoleProfile : Profile
{
    public RoleProfile()
    {
        CreateMap<Role, RoleResponseDto>()
            .ForMember(dest => dest.UserCount, opt => opt.Ignore())
            .ForMember(dest => dest.PermissionCount, opt => opt.Ignore());

        CreateMap<CreateRoleDto, Role>();
        CreateMap<UpdateRoleDto, Role>();
    }
}
