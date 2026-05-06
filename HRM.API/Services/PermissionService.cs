using AutoMapper;
using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.Permission;
using HRM.Core.Entities;
using HRM.Infrastructure.Data;
using HRM.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Services;

public class PermissionService : IPermissionService
{
    private readonly IRepository<Permission> _permissionRepository;
    private readonly IRepository<Role> _roleRepository;
    private readonly IRepository<UserRole> _userRoleRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;
    private readonly AppDbContext _context;

    public PermissionService(
        IRepository<Permission> permissionRepository,
        IRepository<Role> roleRepository,
        IRepository<UserRole> userRoleRepository,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper,
        AppDbContext context)
    {
        _permissionRepository = permissionRepository;
        _roleRepository = roleRepository;
        _userRoleRepository = userRoleRepository;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
        _context = context;
    }

    public async Task<PermissionResponseDto> UpsertAsync(UpsertPermissionDto dto)
    {
        var subscriptionId = GetSubscriptionId();
        var role = await EnsureActiveRoleAsync(dto.RoleId, subscriptionId);

        var moduleCode = dto.ModuleCode.Trim().ToUpperInvariant();
        var now = DateTime.UtcNow;

        var existing = await _permissionRepository.Query()
            .FirstOrDefaultAsync(p =>
                p.RoleId == dto.RoleId &&
                p.ModuleCode == moduleCode &&
                p.SubscriptionId == subscriptionId);

        if (existing is null)
        {
            existing = new Permission
            {
                RoleId = dto.RoleId,
                ModuleCode = moduleCode,
                CanView = dto.CanView,
                CanCreate = dto.CanCreate,
                CanEdit = dto.CanEdit,
                CanDelete = dto.CanDelete,
                CanApprove = dto.CanApprove,
                CanExport = dto.CanExport,
                SubscriptionId = subscriptionId,
                CreatedAt = now,
                UpdatedAt = now
            };
            await _context.Permissions.AddAsync(existing);
        }
        else
        {
            existing.CanView = dto.CanView;
            existing.CanCreate = dto.CanCreate;
            existing.CanEdit = dto.CanEdit;
            existing.CanDelete = dto.CanDelete;
            existing.CanApprove = dto.CanApprove;
            existing.CanExport = dto.CanExport;
            existing.UpdatedAt = now;
        }

        await _context.SaveChangesAsync();

        return MapToResponseDto(existing, role);
    }

    public async Task<IEnumerable<PermissionResponseDto>> BulkUpsertAsync(BulkUpsertPermissionsDto dto)
    {
        var subscriptionId = GetSubscriptionId();
        var role = await EnsureActiveRoleAsync(dto.RoleId, subscriptionId);

        var existing = await _permissionRepository.Query()
            .Where(p => p.RoleId == dto.RoleId && p.SubscriptionId == subscriptionId)
            .ToListAsync();

        foreach (var record in existing)
        {
            _context.Permissions.Remove(record);
        }

        var now = DateTime.UtcNow;
        var newRecords = new List<Permission>();

        foreach (var item in dto.Permissions)
        {
            var moduleCode = item.ModuleCode.Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(moduleCode)) continue;

            // Skip duplicates within the same payload — last write wins per code.
            var duplicate = newRecords.FirstOrDefault(p => p.ModuleCode == moduleCode);
            if (duplicate is not null)
            {
                duplicate.CanView = item.CanView;
                duplicate.CanCreate = item.CanCreate;
                duplicate.CanEdit = item.CanEdit;
                duplicate.CanDelete = item.CanDelete;
                duplicate.CanApprove = item.CanApprove;
                duplicate.CanExport = item.CanExport;
                duplicate.UpdatedAt = now;
                continue;
            }

            newRecords.Add(new Permission
            {
                RoleId = dto.RoleId,
                ModuleCode = moduleCode,
                CanView = item.CanView,
                CanCreate = item.CanCreate,
                CanEdit = item.CanEdit,
                CanDelete = item.CanDelete,
                CanApprove = item.CanApprove,
                CanExport = item.CanExport,
                SubscriptionId = subscriptionId,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        await _context.Permissions.AddRangeAsync(newRecords);
        await _context.SaveChangesAsync();

        return newRecords
            .OrderBy(p => p.ModuleCode)
            .Select(p => MapToResponseDto(p, role))
            .ToList();
    }

    public async Task<IEnumerable<PermissionResponseDto>> GetByRoleAsync(int roleId)
    {
        var subscriptionId = GetSubscriptionId();

        var role = await _roleRepository.GetByIdAsync(roleId)
            ?? throw new KeyNotFoundException($"Role with ID {roleId} not found.");

        if (role.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this role.");
        }

        var perms = await _permissionRepository.Query()
            .AsNoTracking()
            .Where(p => p.RoleId == roleId && p.SubscriptionId == subscriptionId)
            .OrderBy(p => p.ModuleCode)
            .ToListAsync();

        return perms.Select(p => MapToResponseDto(p, role)).ToList();
    }

    public async Task<UserPermissionSummaryDto> GetMyPermissionsAsync()
    {
        var subscriptionId = GetSubscriptionId();
        var userId = GetCallerId();

        var activeRoles = await _userRoleRepository.Query()
            .AsNoTracking()
            .Include(ur => ur.Role)
            .Where(ur =>
                ur.UserId == userId &&
                ur.SubscriptionId == subscriptionId &&
                ur.IsActive &&
                ur.Role.IsActive)
            .ToListAsync();

        var roleIds = activeRoles.Select(ur => ur.RoleId).Distinct().ToList();
        var roleNames = activeRoles.Select(ur => ur.Role.RoleName).Distinct().ToList();

        if (roleIds.Count == 0)
        {
            return new UserPermissionSummaryDto
            {
                UserId = userId,
                Roles = new List<string>(),
                Permissions = new List<PermissionResponseDto>()
            };
        }

        var perms = await _permissionRepository.Query()
            .AsNoTracking()
            .Include(p => p.Role)
            .Where(p =>
                p.SubscriptionId == subscriptionId &&
                roleIds.Contains(p.RoleId))
            .ToListAsync();

        var merged = perms
            .GroupBy(p => p.ModuleCode)
            .Select(g =>
            {
                var first = g.First();
                return new PermissionResponseDto
                {
                    Id = first.Id,
                    RoleId = first.RoleId,
                    RoleName = first.Role?.RoleName ?? string.Empty,
                    ModuleCode = g.Key,
                    ModuleLabel = GetModuleLabel(g.Key),
                    CanView = g.Any(x => x.CanView),
                    CanCreate = g.Any(x => x.CanCreate),
                    CanEdit = g.Any(x => x.CanEdit),
                    CanDelete = g.Any(x => x.CanDelete),
                    CanApprove = g.Any(x => x.CanApprove),
                    CanExport = g.Any(x => x.CanExport),
                    SubscriptionId = subscriptionId,
                    CreatedAt = first.CreatedAt,
                    UpdatedAt = g.Max(x => x.UpdatedAt)
                };
            })
            .OrderBy(p => p.ModuleCode)
            .ToList();

        return new UserPermissionSummaryDto
        {
            UserId = userId,
            Roles = roleNames,
            Permissions = merged
        };
    }

    public async Task<IEnumerable<PermissionResponseDto>> GetAllAsync()
    {
        var subscriptionId = GetSubscriptionId();

        var perms = await _permissionRepository.Query()
            .AsNoTracking()
            .Include(p => p.Role)
            .Where(p => p.SubscriptionId == subscriptionId)
            .OrderBy(p => p.RoleId)
            .ThenBy(p => p.ModuleCode)
            .ToListAsync();

        return perms.Select(p => MapToResponseDto(p, p.Role)).ToList();
    }

    public async Task DeleteAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();

        var permission = await _permissionRepository.Query()
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Permission with ID {id} not found.");

        if (permission.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this permission.");
        }

        await _permissionRepository.DeleteAsync(permission);
    }

    private async Task<Role> EnsureActiveRoleAsync(int roleId, int subscriptionId)
    {
        var role = await _roleRepository.GetByIdAsync(roleId)
            ?? throw new KeyNotFoundException($"Role with ID {roleId} not found.");

        if (role.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this role.");
        }

        if (!role.IsActive)
        {
            throw new InvalidOperationException("Cannot configure permissions for an inactive role.");
        }

        return role;
    }

    private PermissionResponseDto MapToResponseDto(Permission p, Role? role)
    {
        var dto = _mapper.Map<PermissionResponseDto>(p);
        dto.RoleName = role?.RoleName ?? string.Empty;
        dto.ModuleLabel = GetModuleLabel(p.ModuleCode);
        return dto;
    }

    private static string GetModuleLabel(string moduleCode) => moduleCode switch
    {
        "COMPANY" => "Company Management",
        "BRANCH" => "Branch Management",
        "DEPARTMENT" => "Department Management",
        "DESIGNATION" => "Designation Management",
        "EMPLOYEE" => "Employee Management",
        "ATTENDANCE" => "Attendance Management",
        "LEAVE" => "Leave Management",
        "OVERTIME" => "Overtime Management",
        "SALARY" => "Salary Management",
        "BONUS" => "Bonus Management",
        "LOAN" => "Loan Management",
        "PF" => "PF Management",
        "TAX" => "Tax Management",
        "GRATUITY" => "Gratuity Management",
        "SEPARATION" => "Employee Separation",
        "ROLE" => "Role & Permission Management",
        "REPORT" => "Reports & Analytics",
        "USER" => "User Management",
        _ => moduleCode
    };

    private int GetSubscriptionId()
    {
        return _httpContextAccessor.HttpContext?.User.GetSubscriptionId()
            ?? throw new UnauthorizedAccessException("No authenticated user on the request.");
    }

    private int GetCallerId()
    {
        return _httpContextAccessor.HttpContext?.User.GetUserId()
            ?? throw new UnauthorizedAccessException("No authenticated user on the request.");
    }
}
