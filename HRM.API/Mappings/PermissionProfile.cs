using AutoMapper;
using HRM.Core.DTOs.Permission;
using HRM.Core.Entities;

namespace HRM.API.Mappings;

public class PermissionProfile : Profile
{
    public PermissionProfile()
    {
        CreateMap<Permission, PermissionResponseDto>()
            .ForMember(dest => dest.RoleName, opt => opt.Ignore())
            .ForMember(dest => dest.ModuleLabel, opt => opt.Ignore());

        CreateMap<UpsertPermissionDto, Permission>();
    }
}
