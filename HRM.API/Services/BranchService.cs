using AutoMapper;
using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.Branch;
using HRM.Core.Entities;
using HRM.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Services;

public class BranchService : IBranchService
{
    private readonly IRepository<Branch> _branchRepository;
    private readonly IRepository<Company> _companyRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;

    public BranchService(
        IRepository<Branch> branchRepository,
        IRepository<Company> companyRepository,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
    {
        _branchRepository = branchRepository;
        _companyRepository = companyRepository;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
    }

    public async Task<BranchResponseDto> CreateAsync(CreateBranchDto dto)
    {
        var subscriptionId = CurrentSubscriptionId();
        await EnsureCompanyOwnershipAsync(dto.CompanyId, subscriptionId);

        var normalizedCode = dto.Code.Trim().ToLowerInvariant();
        var duplicate = await _branchRepository
            .FindFirstAsync(b =>
                b.CompanyId == dto.CompanyId &&
                b.Code.ToLower() == normalizedCode);

        if (duplicate is not null)
        {
            throw new InvalidOperationException($"A branch with code '{dto.Code}' already exists in this company.");
        }

        var now = DateTime.UtcNow;
        var branch = _mapper.Map<Branch>(dto);
        branch.SubscriptionId = subscriptionId;
        branch.IsActive = true;
        branch.CreatedAt = now;
        branch.UpdatedAt = now;

        await _branchRepository.AddAsync(branch);

        return await LoadResponseAsync(branch.Id, subscriptionId);
    }

    public async Task<BranchResponseDto> GetByIdAsync(int id)
    {
        var subscriptionId = CurrentSubscriptionId();
        return await LoadResponseAsync(id, subscriptionId);
    }

    public async Task<IEnumerable<BranchResponseDto>> GetAllAsync()
    {
        var subscriptionId = CurrentSubscriptionId();

        var branches = await _branchRepository.Query()
            .AsNoTracking()
            .Include(b => b.Company)
            .Where(b => b.SubscriptionId == subscriptionId)
            .OrderBy(b => b.Company.Name)
            .ThenBy(b => b.Name)
            .ToListAsync();

        return _mapper.Map<IEnumerable<BranchResponseDto>>(branches);
    }

    public async Task<IEnumerable<BranchResponseDto>> GetByCompanyAsync(int companyId)
    {
        var subscriptionId = CurrentSubscriptionId();
        await EnsureCompanyOwnershipAsync(companyId, subscriptionId);

        var branches = await _branchRepository.Query()
            .AsNoTracking()
            .Include(b => b.Company)
            .Where(b => b.SubscriptionId == subscriptionId && b.CompanyId == companyId)
            .OrderBy(b => b.Name)
            .ToListAsync();

        return _mapper.Map<IEnumerable<BranchResponseDto>>(branches);
    }

    public async Task<BranchResponseDto> UpdateAsync(int id, UpdateBranchDto dto)
    {
        var subscriptionId = CurrentSubscriptionId();

        var branch = await _branchRepository.Query()
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new KeyNotFoundException($"Branch {id} was not found.");

        if (branch.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("You do not have access to this branch.");
        }

        await EnsureCompanyOwnershipAsync(dto.CompanyId, subscriptionId);

        var normalizedCode = dto.Code.Trim().ToLowerInvariant();
        var duplicate = await _branchRepository.Query()
            .AnyAsync(b =>
                b.Id != id &&
                b.CompanyId == dto.CompanyId &&
                b.Code.ToLower() == normalizedCode);

        if (duplicate)
        {
            throw new InvalidOperationException($"A branch with code '{dto.Code}' already exists in this company.");
        }

        branch.Name = dto.Name;
        branch.Code = dto.Code;
        branch.Address = dto.Address;
        branch.Phone = dto.Phone;
        branch.Email = dto.Email;
        branch.ManagerName = dto.ManagerName;
        branch.CompanyId = dto.CompanyId;
        branch.IsActive = dto.IsActive;
        branch.UpdatedAt = DateTime.UtcNow;

        await _branchRepository.UpdateAsync(branch);

        return await LoadResponseAsync(branch.Id, subscriptionId);
    }

    public async Task DeleteAsync(int id)
    {
        var subscriptionId = CurrentSubscriptionId();

        var branch = await _branchRepository.Query()
            .Include(b => b.Departments)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new KeyNotFoundException($"Branch {id} was not found.");

        if (branch.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("You do not have access to this branch.");
        }

        if (branch.Departments.Any(d => d.IsActive))
        {
            throw new InvalidOperationException("Cannot delete a branch with active departments. Remove all departments first.");
        }

        await _branchRepository.DeleteAsync(branch);
    }

    private async Task<BranchResponseDto> LoadResponseAsync(int branchId, int subscriptionId)
    {
        var branch = await _branchRepository.Query()
            .AsNoTracking()
            .Include(b => b.Company)
            .FirstOrDefaultAsync(b => b.Id == branchId)
            ?? throw new KeyNotFoundException($"Branch {branchId} was not found.");

        if (branch.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("You do not have access to this branch.");
        }

        return _mapper.Map<BranchResponseDto>(branch);
    }

    private async Task EnsureCompanyOwnershipAsync(int companyId, int subscriptionId)
    {
        var company = await _companyRepository.GetByIdAsync(companyId)
            ?? throw new KeyNotFoundException($"Company {companyId} was not found.");

        if (company.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("You do not have access to the specified company.");
        }
    }

    private int CurrentSubscriptionId()
    {
        return _httpContextAccessor.HttpContext?.User.GetSubscriptionId()
            ?? throw new UnauthorizedAccessException("No authenticated user on the request.");
    }
}
