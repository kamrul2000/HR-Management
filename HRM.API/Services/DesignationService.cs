using AutoMapper;
using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.Designation;
using HRM.Core.Entities;
using HRM.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Services;

public class DesignationService : IDesignationService
{
    private readonly IRepository<Designation> _designationRepository;
    private readonly IRepository<Department> _departmentRepository;
    private readonly IRepository<Branch> _branchRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;

    public DesignationService(
        IRepository<Designation> designationRepository,
        IRepository<Department> departmentRepository,
        IRepository<Branch> branchRepository,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
    {
        _designationRepository = designationRepository;
        _departmentRepository = departmentRepository;
        _branchRepository = branchRepository;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
    }

    public async Task<DesignationResponseDto> CreateAsync(CreateDesignationDto dto)
    {
        var subscriptionId = GetSubscriptionId();
        await ResolveAndValidateDepartmentAsync(dto.DepartmentId, subscriptionId);

        var now = DateTime.UtcNow;
        var designation = _mapper.Map<Designation>(dto);
        designation.SubscriptionId = subscriptionId;
        designation.IsActive = true;
        designation.CreatedAt = now;
        designation.UpdatedAt = now;

        await _designationRepository.AddAsync(designation);

        return await LoadResponseAsync(designation.Id, subscriptionId);
    }

    public async Task<DesignationResponseDto> GetByIdAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();
        return await LoadResponseAsync(id, subscriptionId);
    }

    public async Task<IEnumerable<DesignationResponseDto>> GetAllAsync()
    {
        var subscriptionId = GetSubscriptionId();

        var designations = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .OrderBy(d => d.Department.Branch.Company.Name)
            .ThenBy(d => d.Department.Branch.Name)
            .ThenBy(d => d.Department.Name)
            .ThenBy(d => d.Title)
            .ToListAsync();

        return _mapper.Map<IEnumerable<DesignationResponseDto>>(designations);
    }

    public async Task<IEnumerable<DesignationResponseDto>> GetByDepartmentAsync(int departmentId)
    {
        var subscriptionId = GetSubscriptionId();
        await ResolveAndValidateDepartmentAsync(departmentId, subscriptionId);

        var designations = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .Where(d => d.DepartmentId == departmentId)
            .OrderBy(d => d.Title)
            .ToListAsync();

        return _mapper.Map<IEnumerable<DesignationResponseDto>>(designations);
    }

    public async Task<IEnumerable<DesignationResponseDto>> GetByBranchAsync(int branchId)
    {
        var subscriptionId = GetSubscriptionId();
        await EnsureBranchOwnershipAsync(branchId, subscriptionId);

        var designations = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .Where(d => d.Department.BranchId == branchId)
            .OrderBy(d => d.Department.Name)
            .ThenBy(d => d.Title)
            .ToListAsync();

        return _mapper.Map<IEnumerable<DesignationResponseDto>>(designations);
    }

    public async Task<DesignationResponseDto> UpdateAsync(int id, UpdateDesignationDto dto)
    {
        var subscriptionId = GetSubscriptionId();

        var designation = await _designationRepository.Query()
            .FirstOrDefaultAsync(d => d.Id == id)
            ?? throw new KeyNotFoundException($"Designation with ID {id} not found.");

        if (designation.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this designation.");
        }

        if (dto.DepartmentId != designation.DepartmentId)
        {
            await ResolveAndValidateDepartmentAsync(dto.DepartmentId, subscriptionId);
        }

        designation.Title = dto.Title;
        designation.Description = dto.Description;
        designation.Grade = dto.Grade;
        designation.DepartmentId = dto.DepartmentId;
        designation.IsActive = dto.IsActive;
        designation.UpdatedAt = DateTime.UtcNow;

        await _designationRepository.UpdateAsync(designation);

        return await LoadResponseAsync(designation.Id, subscriptionId);
    }

    public async Task DeleteAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();

        var designation = await _designationRepository.Query()
            .Include(d => d.Employees)
            .FirstOrDefaultAsync(d => d.Id == id)
            ?? throw new KeyNotFoundException($"Designation with ID {id} not found.");

        if (designation.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this designation.");
        }

        if (designation.Employees.Any(e => e.IsActive))
        {
            throw new InvalidOperationException("Cannot delete a designation assigned to active employees. Reassign or remove the employees first.");
        }

        await _designationRepository.DeleteAsync(designation);
    }

    private IQueryable<Designation> BaseQuery(int subscriptionId)
    {
        return _designationRepository
            .Query()
            .Include(d => d.Department)
                .ThenInclude(dept => dept.Branch)
                    .ThenInclude(b => b.Company)
            .Where(d => d.SubscriptionId == subscriptionId);
    }

    private async Task<DesignationResponseDto> LoadResponseAsync(int designationId, int subscriptionId)
    {
        var designation = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == designationId);

        if (designation is null)
        {
            var existsForOtherTenant = await _designationRepository.Query()
                .AnyAsync(d => d.Id == designationId);

            if (existsForOtherTenant)
            {
                throw new UnauthorizedAccessException("Access denied to this designation.");
            }

            throw new KeyNotFoundException($"Designation with ID {designationId} not found.");
        }

        return _mapper.Map<DesignationResponseDto>(designation);
    }

    private async Task<Department> ResolveAndValidateDepartmentAsync(int departmentId, int subscriptionId)
    {
        var department = await _departmentRepository
            .Query()
            .Include(d => d.Branch)
                .ThenInclude(b => b.Company)
            .FirstOrDefaultAsync(d => d.Id == departmentId)
            ?? throw new KeyNotFoundException($"Department with ID {departmentId} not found.");

        if (department.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this department.");
        }

        if (!department.IsActive)
        {
            throw new InvalidOperationException("Cannot assign a designation to an inactive department.");
        }

        return department;
    }

    private async Task EnsureBranchOwnershipAsync(int branchId, int subscriptionId)
    {
        var branch = await _branchRepository.GetByIdAsync(branchId)
            ?? throw new KeyNotFoundException($"Branch with ID {branchId} not found.");

        if (branch.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this branch.");
        }
    }

    private int GetSubscriptionId()
    {
        return _httpContextAccessor.HttpContext?.User.GetSubscriptionId()
            ?? throw new UnauthorizedAccessException("No authenticated user on the request.");
    }
}
