using AutoMapper;
using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.UserRole;
using HRM.Core.Entities;
using HRM.Infrastructure.Data;
using HRM.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Services;

public class UserRoleService : IUserRoleService
{
    private readonly IRepository<UserRole> _userRoleRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Role> _roleRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;
    private readonly AppDbContext _context;

    public UserRoleService(
        IRepository<UserRole> userRoleRepository,
        IRepository<User> userRepository,
        IRepository<Role> roleRepository,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper,
        AppDbContext context)
    {
        _userRoleRepository = userRoleRepository;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
        _context = context;
    }

    public async Task<UserRoleResponseDto> AssignAsync(AssignRoleDto dto)
    {
        var subscriptionId = GetSubscriptionId();
        var callerId = GetCallerId();

        var user = await _userRepository.GetByIdAsync(dto.UserId)
            ?? throw new KeyNotFoundException($"User with ID {dto.UserId} not found.");

        if (user.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this user.");
        }

        var role = await _roleRepository.GetByIdAsync(dto.RoleId)
            ?? throw new KeyNotFoundException($"Role with ID {dto.RoleId} not found.");

        if (role.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this role.");
        }

        if (!role.IsActive)
        {
            throw new InvalidOperationException("Cannot assign an inactive role.");
        }

        var alreadyAssigned = await _userRoleRepository.Query()
            .AnyAsync(ur =>
                ur.UserId == dto.UserId &&
                ur.RoleId == dto.RoleId &&
                ur.SubscriptionId == subscriptionId &&
                ur.IsActive);

        if (alreadyAssigned)
        {
            throw new InvalidOperationException("This role is already assigned to the user.");
        }

        var now = DateTime.UtcNow;
        var assignment = new UserRole
        {
            UserId = dto.UserId,
            RoleId = dto.RoleId,
            AssignedAt = now,
            AssignedById = callerId,
            IsActive = true,
            SubscriptionId = subscriptionId,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _context.UserRoles.AddAsync(assignment);
        await _context.SaveChangesAsync();

        return await LoadResponseAsync(assignment.Id, subscriptionId);
    }

    public async Task<UserRoleResponseDto> RevokeAsync(int userRoleId)
    {
        var subscriptionId = GetSubscriptionId();
        var callerId = GetCallerId();

        var assignment = await _userRoleRepository.Query()
            .FirstOrDefaultAsync(ur => ur.Id == userRoleId)
            ?? throw new KeyNotFoundException($"User-role assignment with ID {userRoleId} not found.");

        if (assignment.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this user-role assignment.");
        }

        if (!assignment.IsActive)
        {
            throw new InvalidOperationException("This role assignment is already revoked.");
        }

        var now = DateTime.UtcNow;
        assignment.IsActive = false;
        assignment.RevokedAt = now;
        assignment.RevokedById = callerId;
        assignment.UpdatedAt = now;

        await _context.SaveChangesAsync();

        return await LoadResponseAsync(assignment.Id, subscriptionId);
    }

    public async Task<IEnumerable<UserRoleResponseDto>> GetByUserAsync(int userId)
    {
        var subscriptionId = GetSubscriptionId();
        await EnsureUserOwnershipAsync(userId, subscriptionId);

        var items = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .Where(ur => ur.UserId == userId)
            .OrderByDescending(ur => ur.AssignedAt)
            .ToListAsync();

        return items.Select(MapToResponseDto).ToList();
    }

    public async Task<IEnumerable<UserRoleResponseDto>> GetByRoleAsync(int roleId)
    {
        var subscriptionId = GetSubscriptionId();
        await EnsureRoleOwnershipAsync(roleId, subscriptionId);

        var items = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .Where(ur => ur.RoleId == roleId && ur.IsActive)
            .OrderBy(ur => ur.User.Name)
            .ToListAsync();

        return items.Select(MapToResponseDto).ToList();
    }

    public async Task<IEnumerable<UserRoleResponseDto>> GetAllActiveAsync()
    {
        var subscriptionId = GetSubscriptionId();

        var items = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .Where(ur => ur.IsActive)
            .OrderBy(ur => ur.Role.RoleName)
            .ThenBy(ur => ur.User.Name)
            .ToListAsync();

        return items.Select(MapToResponseDto).ToList();
    }

    private IQueryable<UserRole> BaseQuery(int subscriptionId)
    {
        return _userRoleRepository.Query()
            .Include(ur => ur.User)
            .Include(ur => ur.Role)
            .Where(ur => ur.SubscriptionId == subscriptionId);
    }

    private async Task<UserRoleResponseDto> LoadResponseAsync(int id, int subscriptionId)
    {
        var assignment = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .FirstOrDefaultAsync(ur => ur.Id == id);

        if (assignment is null)
        {
            var existsForOtherTenant = await _userRoleRepository.Query().AnyAsync(ur => ur.Id == id);
            if (existsForOtherTenant)
            {
                throw new UnauthorizedAccessException("Access denied to this user-role assignment.");
            }
            throw new KeyNotFoundException($"User-role assignment with ID {id} not found.");
        }

        return MapToResponseDto(assignment);
    }

    private UserRoleResponseDto MapToResponseDto(UserRole ur)
    {
        var dto = _mapper.Map<UserRoleResponseDto>(ur);
        dto.UserName = ur.User?.Name ?? string.Empty;
        dto.UserEmail = ur.User?.Email ?? string.Empty;
        dto.RoleName = ur.Role?.RoleName ?? string.Empty;
        dto.AssignedAtFormatted = ur.AssignedAt.ToString("dd MMM yyyy HH:mm");
        dto.RevokedAtFormatted = ur.RevokedAt?.ToString("dd MMM yyyy HH:mm");
        return dto;
    }

    private async Task EnsureUserOwnershipAsync(int userId, int subscriptionId)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException($"User with ID {userId} not found.");

        if (user.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this user.");
        }
    }

    private async Task EnsureRoleOwnershipAsync(int roleId, int subscriptionId)
    {
        var role = await _roleRepository.GetByIdAsync(roleId)
            ?? throw new KeyNotFoundException($"Role with ID {roleId} not found.");

        if (role.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this role.");
        }
    }

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
