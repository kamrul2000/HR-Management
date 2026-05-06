using AutoMapper;
using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.Role;
using HRM.Core.Entities;
using HRM.Infrastructure.Data;
using HRM.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Services;

public class RoleService : IRoleService
{
    private readonly IRepository<Role> _roleRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;
    private readonly AppDbContext _context;

    public RoleService(
        IRepository<Role> roleRepository,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper,
        AppDbContext context)
    {
        _roleRepository = roleRepository;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
        _context = context;
    }

    public async Task<RoleResponseDto> CreateAsync(CreateRoleDto dto)
    {
        var subscriptionId = GetSubscriptionId();
        var trimmed = dto.RoleName.Trim();

        await EnsureUniqueAsync(trimmed, subscriptionId);

        var now = DateTime.UtcNow;
        var role = new Role
        {
            RoleName = trimmed,
            Description = dto.Description,
            IsActive = true,
            SubscriptionId = subscriptionId,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _context.Roles.AddAsync(role);
        await _context.SaveChangesAsync();

        return await LoadResponseAsync(role.Id, subscriptionId);
    }

    public async Task<RoleResponseDto> GetByIdAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();
        return await LoadResponseAsync(id, subscriptionId);
    }

    public async Task<IEnumerable<RoleResponseDto>> GetAllAsync()
    {
        var subscriptionId = GetSubscriptionId();

        var roles = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .OrderBy(r => r.RoleName)
            .ToListAsync();

        return roles.Select(MapToResponseDto).ToList();
    }

    public async Task<IEnumerable<RoleResponseDto>> GetActiveAsync()
    {
        var subscriptionId = GetSubscriptionId();

        var roles = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .Where(r => r.IsActive)
            .OrderBy(r => r.RoleName)
            .ToListAsync();

        return roles.Select(MapToResponseDto).ToList();
    }

    public async Task<RoleResponseDto> UpdateAsync(int id, UpdateRoleDto dto)
    {
        var subscriptionId = GetSubscriptionId();
        var trimmed = dto.RoleName.Trim();

        var role = await LoadRoleAsync(id, subscriptionId);

        if (!string.Equals(role.RoleName, trimmed, StringComparison.Ordinal))
        {
            await EnsureUniqueAsync(trimmed, subscriptionId, id);
        }

        role.RoleName = trimmed;
        role.Description = dto.Description;
        role.IsActive = dto.IsActive;
        role.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await LoadResponseAsync(role.Id, subscriptionId);
    }

    public async Task DeleteAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();
        var role = await LoadRoleAsync(id, subscriptionId);

        var hasActiveAssignments = await _context.UserRoles
            .AnyAsync(ur => ur.RoleId == id && ur.IsActive);

        if (hasActiveAssignments)
        {
            throw new InvalidOperationException("Cannot delete a role assigned to users.");
        }

        var hasPermissions = await _context.Permissions
            .AnyAsync(p => p.RoleId == id);

        if (hasPermissions)
        {
            throw new InvalidOperationException("Cannot delete a role that has permissions configured. Remove permissions first.");
        }

        await _roleRepository.DeleteAsync(role);
    }

    private IQueryable<Role> BaseQuery(int subscriptionId)
    {
        return _roleRepository.Query()
            .Include(r => r.UserRoles)
            .Include(r => r.Permissions)
            .Where(r => r.SubscriptionId == subscriptionId);
    }

    private async Task<Role> LoadRoleAsync(int id, int subscriptionId)
    {
        var role = await _roleRepository.Query()
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role is null)
        {
            throw new KeyNotFoundException($"Role with ID {id} not found.");
        }

        if (role.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this role.");
        }

        return role;
    }

    private async Task<RoleResponseDto> LoadResponseAsync(int id, int subscriptionId)
    {
        var role = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role is null)
        {
            var existsForOtherTenant = await _roleRepository.Query().AnyAsync(r => r.Id == id);
            if (existsForOtherTenant)
            {
                throw new UnauthorizedAccessException("Access denied to this role.");
            }
            throw new KeyNotFoundException($"Role with ID {id} not found.");
        }

        return MapToResponseDto(role);
    }

    private async Task EnsureUniqueAsync(string roleName, int subscriptionId, int? excludeId = null)
    {
        var exists = await _roleRepository.Query()
            .AnyAsync(r =>
                r.RoleName == roleName &&
                r.SubscriptionId == subscriptionId &&
                (excludeId == null || r.Id != excludeId));

        if (exists)
        {
            throw new InvalidOperationException($"A role named '{roleName}' already exists.");
        }
    }

    private RoleResponseDto MapToResponseDto(Role role)
    {
        var dto = _mapper.Map<RoleResponseDto>(role);
        dto.UserCount = role.UserRoles?.Count(ur => ur.IsActive) ?? 0;
        dto.PermissionCount = role.Permissions?.Count ?? 0;
        return dto;
    }

    private int GetSubscriptionId()
    {
        return _httpContextAccessor.HttpContext?.User.GetSubscriptionId()
            ?? throw new UnauthorizedAccessException("No authenticated user on the request.");
    }
}
