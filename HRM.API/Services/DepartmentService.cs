using AutoMapper;
using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.Department;
using HRM.Core.Entities;
using HRM.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IRepository<Department> _departmentRepository;
    private readonly IRepository<Branch> _branchRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;

    public DepartmentService(
        IRepository<Department> departmentRepository,
        IRepository<Branch> branchRepository,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
    {
        _departmentRepository = departmentRepository;
        _branchRepository = branchRepository;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
    }

    public async Task<DepartmentResponseDto> CreateAsync(CreateDepartmentDto dto)
    {
        var subscriptionId = GetSubscriptionId();
        await ResolveAndValidateBranchAsync(dto.BranchId, subscriptionId);

        var now = DateTime.UtcNow;
        var department = _mapper.Map<Department>(dto);
        department.SubscriptionId = subscriptionId;
        department.IsActive = true;
        department.CreatedAt = now;
        department.UpdatedAt = now;

        await _departmentRepository.AddAsync(department);

        return await LoadResponseAsync(department.Id, subscriptionId);
    }

    public async Task<DepartmentResponseDto> GetByIdAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();
        return await LoadResponseAsync(id, subscriptionId);
    }

    public async Task<IEnumerable<DepartmentResponseDto>> GetAllAsync()
    {
        var subscriptionId = GetSubscriptionId();

        var departments = await _departmentRepository.Query()
            .AsNoTracking()
            .Include(d => d.Branch)
                .ThenInclude(b => b.Company)
            .Where(d => d.SubscriptionId == subscriptionId)
            .OrderBy(d => d.Branch.Name)
            .ThenBy(d => d.Name)
            .ToListAsync();

        return _mapper.Map<IEnumerable<DepartmentResponseDto>>(departments);
    }

    public async Task<IEnumerable<DepartmentResponseDto>> GetByBranchAsync(int branchId)
    {
        var subscriptionId = GetSubscriptionId();
        await ResolveAndValidateBranchAsync(branchId, subscriptionId);

        var departments = await _departmentRepository.Query()
            .AsNoTracking()
            .Include(d => d.Branch)
                .ThenInclude(b => b.Company)
            .Where(d => d.SubscriptionId == subscriptionId && d.BranchId == branchId)
            .OrderBy(d => d.Name)
            .ToListAsync();

        return _mapper.Map<IEnumerable<DepartmentResponseDto>>(departments);
    }

    public async Task<DepartmentResponseDto> UpdateAsync(int id, UpdateDepartmentDto dto)
    {
        var subscriptionId = GetSubscriptionId();

        var department = await _departmentRepository.Query()
            .FirstOrDefaultAsync(d => d.Id == id)
            ?? throw new KeyNotFoundException($"Department with ID {id} not found.");

        if (department.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this department.");
        }

        if (dto.BranchId != department.BranchId)
        {
            await ResolveAndValidateBranchAsync(dto.BranchId, subscriptionId);
        }

        department.Name = dto.Name;
        department.Description = dto.Description;
        department.BranchId = dto.BranchId;
        department.IsActive = dto.IsActive;
        department.UpdatedAt = DateTime.UtcNow;

        await _departmentRepository.UpdateAsync(department);

        return await LoadResponseAsync(department.Id, subscriptionId);
    }

    public async Task DeleteAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();

        var department = await _departmentRepository.Query()
            .Include(d => d.Designations)
            .FirstOrDefaultAsync(d => d.Id == id)
            ?? throw new KeyNotFoundException($"Department with ID {id} not found.");

        if (department.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this department.");
        }

        if (department.Designations.Any(des => des.IsActive))
        {
            throw new InvalidOperationException("Cannot delete a department with active designations. Remove all designations first.");
        }

        await _departmentRepository.DeleteAsync(department);
    }

    private async Task<DepartmentResponseDto> LoadResponseAsync(int departmentId, int subscriptionId)
    {
        var department = await _departmentRepository.Query()
            .AsNoTracking()
            .Include(d => d.Branch)
                .ThenInclude(b => b.Company)
            .FirstOrDefaultAsync(d => d.Id == departmentId)
            ?? throw new KeyNotFoundException($"Department with ID {departmentId} not found.");

        if (department.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this department.");
        }

        return _mapper.Map<DepartmentResponseDto>(department);
    }

    private async Task<Branch> ResolveAndValidateBranchAsync(int branchId, int subscriptionId)
    {
        var branch = await _branchRepository.GetByIdAsync(branchId)
            ?? throw new KeyNotFoundException($"Branch with ID {branchId} not found.");

        if (branch.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this branch.");
        }

        if (!branch.IsActive)
        {
            throw new InvalidOperationException("Cannot assign a department to an inactive branch.");
        }

        return branch;
    }

    private int GetSubscriptionId()
    {
        return _httpContextAccessor.HttpContext?.User.GetSubscriptionId()
            ?? throw new UnauthorizedAccessException("No authenticated user on the request.");
    }
}
