using AutoMapper;
using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.SalaryCreate;
using HRM.Core.Entities;
using HRM.Infrastructure.Data;
using HRM.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Services;

public class SalaryCreateService : ISalaryCreateService
{
    private readonly IRepository<SalaryStructure> _structureRepository;
    private readonly IRepository<SalaryStructureItem> _itemRepository;
    private readonly IRepository<Employee> _employeeRepository;
    private readonly IRepository<SalaryHead> _salaryHeadRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;
    private readonly AppDbContext _context;

    public SalaryCreateService(
        IRepository<SalaryStructure> structureRepository,
        IRepository<SalaryStructureItem> itemRepository,
        IRepository<Employee> employeeRepository,
        IRepository<SalaryHead> salaryHeadRepository,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper,
        AppDbContext context)
    {
        _structureRepository = structureRepository;
        _itemRepository = itemRepository;
        _employeeRepository = employeeRepository;
        _salaryHeadRepository = salaryHeadRepository;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
        _context = context;
    }

    public async Task<SalaryStructureResponseDto> CreateAsync(CreateSalaryStructureDto dto)
    {
        var subscriptionId = GetSubscriptionId();
        var employee = await ResolveEmployeeAsync(dto.EmployeeId, subscriptionId);

        var effectiveFrom = dto.EffectiveFrom.Date;

        if (dto.BasicSalary <= 0)
        {
            throw new InvalidOperationException("BasicSalary must be greater than zero.");
        }

        await ResolveAndValidateItemsAsync(dto.Items, subscriptionId);

        var existingActive = await _structureRepository.Query()
            .FirstOrDefaultAsync(s =>
                s.EmployeeId == employee.Id &&
                s.SubscriptionId == subscriptionId &&
                s.IsActive);

        var now = DateTime.UtcNow;

        if (existingActive is not null)
        {
            if (existingActive.EffectiveFrom >= effectiveFrom)
            {
                throw new InvalidOperationException(
                    $"New effective date must be after existing structure effective date ({existingActive.EffectiveFrom:dd MMM yyyy}).");
            }

            existingActive.EffectiveTo = effectiveFrom.AddDays(-1);
            existingActive.IsActive = false;
            existingActive.UpdatedAt = now;
        }

        var structure = new SalaryStructure
        {
            EmployeeId = employee.Id,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = null,
            BasicSalary = dto.BasicSalary,
            IsActive = true,
            Remarks = dto.Remarks,
            SubscriptionId = subscriptionId,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _context.SalaryStructures.AddAsync(structure);
        await _context.SaveChangesAsync();

        foreach (var itemDto in dto.Items)
        {
            var item = new SalaryStructureItem
            {
                SalaryStructureId = structure.Id,
                SalaryHeadId = itemDto.SalaryHeadId,
                FixedAmount = itemDto.FixedAmount,
                OverridePercentage = itemDto.OverridePercentage,
                SubscriptionId = subscriptionId,
                CreatedAt = now,
                UpdatedAt = now
            };
            await _context.SalaryStructureItems.AddAsync(item);
        }

        await _context.SaveChangesAsync();

        return await LoadResponseAsync(structure.Id, subscriptionId);
    }

    public async Task<SalaryStructureResponseDto> GetByIdAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();
        return await LoadResponseAsync(id, subscriptionId);
    }

    public async Task<SalaryStructureResponseDto> GetActiveByEmployeeAsync(int employeeId)
    {
        var subscriptionId = GetSubscriptionId();
        await EnsureEmployeeOwnershipAsync(employeeId, subscriptionId);

        var structure = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.EmployeeId == employeeId && s.IsActive)
            ?? throw new KeyNotFoundException("No active salary structure found for this employee.");

        return MapToResponseDto(structure);
    }

    public async Task<IEnumerable<SalaryStructureHistoryDto>> GetHistoryByEmployeeAsync(int employeeId)
    {
        var subscriptionId = GetSubscriptionId();
        await EnsureEmployeeOwnershipAsync(employeeId, subscriptionId);

        var structures = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .Where(s => s.EmployeeId == employeeId)
            .OrderByDescending(s => s.EffectiveFrom)
            .ToListAsync();

        return structures.Select(s =>
        {
            var (gross, _, _) = ComputeEstimatedSalary(s.BasicSalary, s.Items.ToList());
            return new SalaryStructureHistoryDto
            {
                Id = s.Id,
                EffectiveFrom = s.EffectiveFrom,
                EffectiveFromFormatted = s.EffectiveFrom.ToString("dd MMM yyyy"),
                EffectiveTo = s.EffectiveTo,
                EffectiveToFormatted = s.EffectiveTo?.ToString("dd MMM yyyy"),
                BasicSalary = s.BasicSalary,
                EstimatedGrossSalary = gross,
                IsActive = s.IsActive,
                Remarks = s.Remarks,
                CreatedAt = s.CreatedAt
            };
        }).ToList();
    }

    public async Task<IEnumerable<SalaryStructureResponseDto>> GetAllActiveAsync()
    {
        var subscriptionId = GetSubscriptionId();

        var structures = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.Employee.FullName)
            .ToListAsync();

        return structures.Select(MapToResponseDto).ToList();
    }

    public async Task<SalaryStructureResponseDto> UpdateAsync(int id, UpdateSalaryStructureDto dto)
    {
        var subscriptionId = GetSubscriptionId();

        var structure = await _structureRepository.Query()
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new KeyNotFoundException($"Salary structure with ID {id} not found.");

        if (structure.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this salary structure.");
        }

        if (!structure.IsActive)
        {
            throw new InvalidOperationException(
                "Only the active salary structure can be updated. Create a new structure revision instead.");
        }

        await ResolveAndValidateItemsAsync(dto.Items, subscriptionId);

        var now = DateTime.UtcNow;

        var existingItems = structure.Items.ToList();
        foreach (var existingItem in existingItems)
        {
            _context.SalaryStructureItems.Remove(existingItem);
        }

        foreach (var itemDto in dto.Items)
        {
            var item = new SalaryStructureItem
            {
                SalaryStructureId = structure.Id,
                SalaryHeadId = itemDto.SalaryHeadId,
                FixedAmount = itemDto.FixedAmount,
                OverridePercentage = itemDto.OverridePercentage,
                SubscriptionId = subscriptionId,
                CreatedAt = now,
                UpdatedAt = now
            };
            await _context.SalaryStructureItems.AddAsync(item);
        }

        structure.Remarks = dto.Remarks;
        structure.UpdatedAt = now;

        await _context.SaveChangesAsync();

        return await LoadResponseAsync(structure.Id, subscriptionId);
    }

    public async Task<SalaryStructureResponseDto?> GetActiveByEmployeeInternalAsync(int employeeId, int subscriptionId)
    {
        var structure = await _structureRepository.Query()
            .AsNoTracking()
            .Include(s => s.Employee)
            .Include(s => s.Items)
                .ThenInclude(i => i.SalaryHead)
            .Where(s =>
                s.EmployeeId == employeeId &&
                s.SubscriptionId == subscriptionId &&
                s.IsActive)
            .FirstOrDefaultAsync();

        return structure is null ? null : MapToResponseDto(structure);
    }

    public async Task<SalaryStructure?> GetStructureActiveOnDateAsync(int employeeId, DateTime date)
    {
        var subscriptionId = GetSubscriptionId();
        var target = date.Date;

        return await _structureRepository.Query()
            .AsNoTracking()
            .Include(s => s.Items)
                .ThenInclude(i => i.SalaryHead)
            .Where(s =>
                s.EmployeeId == employeeId &&
                s.SubscriptionId == subscriptionId &&
                s.EffectiveFrom <= target &&
                (s.EffectiveTo == null || s.EffectiveTo >= target))
            .OrderByDescending(s => s.EffectiveFrom)
            .FirstOrDefaultAsync();
    }

    public async Task DeactivateAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();

        var structure = await _structureRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Salary structure with ID {id} not found.");

        if (structure.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this salary structure.");
        }

        if (!structure.IsActive)
        {
            throw new InvalidOperationException("This salary structure is already inactive.");
        }

        var now = DateTime.UtcNow;
        structure.IsActive = false;
        structure.EffectiveTo = now.Date;
        structure.UpdatedAt = now;

        await _structureRepository.UpdateAsync(structure);
    }

    private IQueryable<SalaryStructure> BaseQuery(int subscriptionId)
    {
        return _structureRepository
            .Query()
            .Include(s => s.Employee)
            .Include(s => s.Items)
                .ThenInclude(i => i.SalaryHead)
            .Where(s => s.SubscriptionId == subscriptionId);
    }

    private async Task<SalaryStructureResponseDto> LoadResponseAsync(int structureId, int subscriptionId)
    {
        var structure = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == structureId);

        if (structure is null)
        {
            var existsForOtherTenant = await _structureRepository.Query()
                .AnyAsync(s => s.Id == structureId);

            if (existsForOtherTenant)
            {
                throw new UnauthorizedAccessException("Access denied to this salary structure.");
            }

            throw new KeyNotFoundException($"Salary structure with ID {structureId} not found.");
        }

        return MapToResponseDto(structure);
    }

    private SalaryStructureResponseDto MapToResponseDto(SalaryStructure s)
    {
        var dto = _mapper.Map<SalaryStructureResponseDto>(s);

        dto.EffectiveFromFormatted = s.EffectiveFrom.ToString("dd MMM yyyy");
        dto.EffectiveToFormatted = s.EffectiveTo?.ToString("dd MMM yyyy");
        dto.BasicSalaryFormatted = s.BasicSalary.ToString("N2");

        var (gross, deductions, net) = ComputeEstimatedSalary(s.BasicSalary, s.Items.ToList());
        dto.EstimatedGrossSalary = gross;
        dto.EstimatedDeductions = deductions;
        dto.EstimatedNetSalary = net;

        dto.Items = s.Items
            .OrderBy(i => i.SalaryHead.HeadType)
            .ThenBy(i => i.SalaryHead.DisplayOrder)
            .Select(i => new SalaryStructureItemResponseDto
            {
                Id = i.Id,
                SalaryHeadId = i.SalaryHeadId,
                HeadName = i.SalaryHead.HeadName,
                HeadCode = i.SalaryHead.HeadCode,
                HeadType = i.SalaryHead.HeadType,
                HeadTypeLabel = i.SalaryHead.HeadType == "Earning" ? "Earning (+)" : "Deduction (-)",
                CalculationMethod = i.SalaryHead.CalculationMethod,
                FixedAmount = i.FixedAmount,
                OverridePercentage = i.OverridePercentage,
                EffectivePercentage = i.OverridePercentage ?? i.SalaryHead.Percentage,
                IsTaxable = i.SalaryHead.IsTaxable,
                IsProvidentFundApplicable = i.SalaryHead.IsProvidentFundApplicable,
                DisplayOrder = i.SalaryHead.DisplayOrder
            })
            .ToList();

        return dto;
    }

    private async Task ResolveAndValidateItemsAsync(List<SalaryStructureItemDto> items, int subscriptionId)
    {
        if (items.Count == 0)
        {
            throw new InvalidOperationException("Salary structure must contain at least one item.");
        }

        var duplicateHeads = items
            .GroupBy(i => i.SalaryHeadId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateHeads.Count > 0)
        {
            throw new InvalidOperationException(
                $"Duplicate salary heads in request: {string.Join(", ", duplicateHeads)}.");
        }

        var resolvedHeads = new List<SalaryHead>();

        foreach (var item in items)
        {
            var head = await _salaryHeadRepository.Query()
                .FirstOrDefaultAsync(h =>
                    h.Id == item.SalaryHeadId &&
                    h.SubscriptionId == subscriptionId)
                ?? throw new KeyNotFoundException(
                    $"SalaryHead with ID {item.SalaryHeadId} not found.");

            if (!head.IsActive)
            {
                throw new InvalidOperationException(
                    $"SalaryHead '{head.HeadName}' is inactive and cannot be included.");
            }

            if (head.CalculationMethod == "Fixed")
            {
                if (!item.FixedAmount.HasValue || item.FixedAmount.Value <= 0)
                {
                    throw new InvalidOperationException(
                        $"'{head.HeadName}' uses Fixed calculation — FixedAmount must be provided and > 0.");
                }
            }

            if (item.OverridePercentage.HasValue && head.CalculationMethod == "Fixed")
            {
                throw new InvalidOperationException(
                    $"OverridePercentage cannot be set on '{head.HeadName}' which uses Fixed calculation.");
            }

            resolvedHeads.Add(head);
        }

        bool hasBasic = resolvedHeads.Any(h => h.HeadCode == "BASIC");
        if (!hasBasic)
        {
            throw new InvalidOperationException(
                "Salary structure must include the 'BASIC' salary head (HeadCode = 'BASIC'). " +
                "It is required as the base for percentage calculations.");
        }
    }

    private (decimal estimatedGross, decimal estimatedDeductions, decimal estimatedNet)
        ComputeEstimatedSalary(decimal basicSalary, List<SalaryStructureItem> items)
    {
        decimal gross = 0m;
        decimal deductions = 0m;

        foreach (var item in items.OrderBy(i => i.SalaryHead.DisplayOrder))
        {
            if (item.SalaryHead.HeadType != "Earning") continue;

            decimal value = item.SalaryHead.CalculationMethod switch
            {
                "Fixed" => item.FixedAmount ?? 0m,
                "PercentageOfBasic" => basicSalary *
                    (item.OverridePercentage ?? item.SalaryHead.Percentage ?? 0m) / 100m,
                _ => item.FixedAmount ?? 0m
            };
            gross += value;
        }

        foreach (var item in items.OrderBy(i => i.SalaryHead.DisplayOrder))
        {
            if (item.SalaryHead.HeadType != "Deduction") continue;

            decimal value = item.SalaryHead.CalculationMethod switch
            {
                "Fixed" => item.FixedAmount ?? 0m,
                "PercentageOfBasic" => basicSalary *
                    (item.OverridePercentage ?? item.SalaryHead.Percentage ?? 0m) / 100m,
                "PercentageOfGross" => gross *
                    (item.OverridePercentage ?? item.SalaryHead.Percentage ?? 0m) / 100m,
                _ => item.FixedAmount ?? 0m
            };
            deductions += value;
        }

        decimal net = gross - deductions;
        return (Math.Round(gross, 2), Math.Round(deductions, 2), Math.Round(net, 2));
    }

    private async Task<Employee> ResolveEmployeeAsync(int employeeId, int subscriptionId)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId)
            ?? throw new KeyNotFoundException($"Employee with ID {employeeId} not found.");

        if (employee.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this employee.");
        }

        if (!employee.IsActive || employee.Status != "Active")
        {
            throw new InvalidOperationException(
                "Salary structures can only be assigned to active employees.");
        }

        return employee;
    }

    private async Task EnsureEmployeeOwnershipAsync(int employeeId, int subscriptionId)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId)
            ?? throw new KeyNotFoundException($"Employee with ID {employeeId} not found.");

        if (employee.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this employee.");
        }
    }

    private int GetSubscriptionId()
    {
        return _httpContextAccessor.HttpContext?.User.GetSubscriptionId()
            ?? throw new UnauthorizedAccessException("No authenticated user on the request.");
    }
}
