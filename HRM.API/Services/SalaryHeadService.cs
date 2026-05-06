using AutoMapper;
using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.SalaryHead;
using HRM.Core.Entities;
using HRM.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Services;

public class SalaryHeadService : ISalaryHeadService
{
    private static readonly string[] ValidHeadTypes = { "Earning", "Deduction" };

    private static readonly string[] ValidMethods =
    {
        "Fixed", "PercentageOfBasic", "PercentageOfGross",
        "PercentageOfHead", "PercentageOfNet", "Formula"
    };

    private readonly IRepository<SalaryHead> _salaryHeadRepository;
    private readonly IRepository<SalaryCreate> _salaryCreateRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;

    public SalaryHeadService(
        IRepository<SalaryHead> salaryHeadRepository,
        IRepository<SalaryCreate> salaryCreateRepository,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
    {
        _salaryHeadRepository = salaryHeadRepository;
        _salaryCreateRepository = salaryCreateRepository;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
    }

    public async Task<SalaryHeadResponseDto> CreateAsync(CreateSalaryHeadDto dto)
    {
        var subscriptionId = GetSubscriptionId();

        ValidateHeadType(dto.HeadType);
        ValidateCalculationMethod(dto.CalculationMethod);
        await ValidateCalculationConfigAsync(
            dto.CalculationMethod, dto.Percentage, dto.BaseHeadId, subscriptionId);

        var headName = dto.HeadName.Trim();
        var headCode = dto.HeadCode.Trim().ToUpperInvariant();

        await EnsureUniqueAsync(headName, headCode, subscriptionId);

        var now = DateTime.UtcNow;
        var head = _mapper.Map<SalaryHead>(dto);
        head.HeadName = headName;
        head.HeadCode = headCode;
        head.SubscriptionId = subscriptionId;
        head.IsActive = true;
        head.CreatedAt = now;
        head.UpdatedAt = now;

        await _salaryHeadRepository.AddAsync(head);

        return await LoadResponseAsync(head.Id, subscriptionId);
    }

    public async Task<SalaryHeadResponseDto> GetByIdAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();
        return await LoadResponseAsync(id, subscriptionId);
    }

    public async Task<IEnumerable<SalaryHeadResponseDto>> GetAllAsync()
    {
        var subscriptionId = GetSubscriptionId();

        var heads = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .OrderBy(h => h.HeadType)
            .ThenBy(h => h.DisplayOrder)
            .ToListAsync();

        return heads.Select(MapToResponseDto).ToList();
    }

    public async Task<IEnumerable<SalaryHeadResponseDto>> GetEarningsAsync()
    {
        var subscriptionId = GetSubscriptionId();

        var heads = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .Where(h => h.HeadType == "Earning" && h.IsActive)
            .OrderBy(h => h.DisplayOrder)
            .ToListAsync();

        return heads.Select(MapToResponseDto).ToList();
    }

    public async Task<IEnumerable<SalaryHeadResponseDto>> GetDeductionsAsync()
    {
        var subscriptionId = GetSubscriptionId();

        var heads = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .Where(h => h.HeadType == "Deduction" && h.IsActive)
            .OrderBy(h => h.DisplayOrder)
            .ToListAsync();

        return heads.Select(MapToResponseDto).ToList();
    }

    public async Task<IEnumerable<SalaryHeadSummaryDto>> GetActiveHeadsSummaryAsync()
    {
        var subscriptionId = GetSubscriptionId();

        var heads = await _salaryHeadRepository.Query()
            .AsNoTracking()
            .Where(h => h.SubscriptionId == subscriptionId && h.IsActive)
            .OrderBy(h => h.HeadType)
            .ThenBy(h => h.DisplayOrder)
            .ToListAsync();

        return heads.Select(h => _mapper.Map<SalaryHeadSummaryDto>(h)).ToList();
    }

    public async Task<SalaryHeadResponseDto> UpdateAsync(int id, UpdateSalaryHeadDto dto)
    {
        var subscriptionId = GetSubscriptionId();

        var head = await _salaryHeadRepository.Query()
            .FirstOrDefaultAsync(h => h.Id == id)
            ?? throw new KeyNotFoundException($"Salary head with ID {id} not found.");

        if (head.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this salary head.");
        }

        ValidateHeadType(dto.HeadType);
        ValidateCalculationMethod(dto.CalculationMethod);
        await ValidateCalculationConfigAsync(
            dto.CalculationMethod, dto.Percentage, dto.BaseHeadId, subscriptionId, selfId: id);

        var headName = dto.HeadName.Trim();
        var headCode = dto.HeadCode.Trim().ToUpperInvariant();

        var nameChanged = !string.Equals(headName, head.HeadName, StringComparison.OrdinalIgnoreCase);
        var codeChanged = !string.Equals(headCode, head.HeadCode, StringComparison.OrdinalIgnoreCase);

        if (nameChanged || codeChanged)
        {
            await EnsureUniqueAsync(headName, headCode, subscriptionId, excludeId: id);
        }

        head.HeadName = headName;
        head.HeadCode = headCode;
        head.HeadType = dto.HeadType;
        head.CalculationMethod = dto.CalculationMethod;
        head.Percentage = dto.Percentage;
        head.BaseHeadId = dto.BaseHeadId;
        head.IsFixed = dto.IsFixed;
        head.IsTaxable = dto.IsTaxable;
        head.IsProvidentFundApplicable = dto.IsProvidentFundApplicable;
        head.DisplayOrder = dto.DisplayOrder;
        head.Description = dto.Description;
        head.IsActive = dto.IsActive;
        head.UpdatedAt = DateTime.UtcNow;

        await _salaryHeadRepository.UpdateAsync(head);

        return await LoadResponseAsync(head.Id, subscriptionId);
    }

    public async Task DeleteAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();

        var head = await _salaryHeadRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Salary head with ID {id} not found.");

        if (head.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this salary head.");
        }

        var hasDependentHeads = await _salaryHeadRepository.Query()
            .AnyAsync(h => h.BaseHeadId == id);

        if (hasDependentHeads)
        {
            throw new InvalidOperationException(
                "Cannot delete a salary head that is used as a base head for other heads. Update the dependent heads first.");
        }

        var hasSalaryStructureRefs = await _salaryCreateRepository.Query()
            .AnyAsync(s => s.SalaryHeadId == id);

        if (hasSalaryStructureRefs)
        {
            throw new InvalidOperationException(
                "Cannot delete a salary head assigned to employee salary structures.");
        }

        await _salaryHeadRepository.DeleteAsync(head);
    }

    private IQueryable<SalaryHead> BaseQuery(int subscriptionId)
    {
        return _salaryHeadRepository
            .Query()
            .Include(h => h.BaseHead)
            .Where(h => h.SubscriptionId == subscriptionId);
    }

    private async Task<SalaryHeadResponseDto> LoadResponseAsync(int headId, int subscriptionId)
    {
        var head = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.Id == headId);

        if (head is null)
        {
            var existsForOtherTenant = await _salaryHeadRepository.Query()
                .AnyAsync(h => h.Id == headId);

            if (existsForOtherTenant)
            {
                throw new UnauthorizedAccessException("Access denied to this salary head.");
            }

            throw new KeyNotFoundException($"Salary head with ID {headId} not found.");
        }

        return MapToResponseDto(head);
    }

    private SalaryHeadResponseDto MapToResponseDto(SalaryHead h)
    {
        var dto = _mapper.Map<SalaryHeadResponseDto>(h);

        dto.HeadTypeLabel = h.HeadType == "Earning" ? "Earning (+)" : "Deduction (-)";

        dto.CalculationMethodLabel = h.CalculationMethod switch
        {
            "Fixed" => "Fixed Amount",
            "PercentageOfBasic" => "% of Basic Salary",
            "PercentageOfGross" => "% of Gross Salary",
            "PercentageOfHead" => h.BaseHead is not null
                ? $"% of {h.BaseHead.HeadName}"
                : "% of Head",
            "PercentageOfNet" => "% of Net Salary ⚠",
            "Formula" => "Custom Formula",
            _ => h.CalculationMethod
        };

        dto.CalculationWarning = h.CalculationMethod == "PercentageOfNet"
            ? "Warning: Percentage of Net Salary can cause circular calculation dependencies. " +
              "Use with caution and verify salary calculation results."
            : null;

        dto.BaseHeadName = h.BaseHead?.HeadName;

        return dto;
    }

    private async Task ValidateCalculationConfigAsync(
        string method, decimal? percentage, int? baseHeadId,
        int subscriptionId, int? selfId = null)
    {
        bool requiresPercentage = method is
            "PercentageOfBasic" or "PercentageOfGross" or
            "PercentageOfHead" or "PercentageOfNet";

        bool requiresBaseHead = method == "PercentageOfHead";
        bool forbidsPercentage = method is "Fixed" or "Formula";

        if (forbidsPercentage && percentage.HasValue)
        {
            throw new InvalidOperationException(
                $"CalculationMethod '{method}' must not include a Percentage value.");
        }

        if (requiresPercentage && !percentage.HasValue)
        {
            throw new InvalidOperationException(
                $"CalculationMethod '{method}' requires a Percentage value.");
        }

        if (!requiresBaseHead && baseHeadId.HasValue)
        {
            throw new InvalidOperationException(
                $"CalculationMethod '{method}' must not include a BaseHeadId.");
        }

        if (requiresBaseHead)
        {
            if (!baseHeadId.HasValue)
            {
                throw new InvalidOperationException(
                    "CalculationMethod 'PercentageOfHead' requires a BaseHeadId.");
            }

            if (selfId.HasValue && baseHeadId.Value == selfId.Value)
            {
                throw new InvalidOperationException(
                    "A salary head cannot reference itself as its base head.");
            }

            var baseHead = await _salaryHeadRepository.Query()
                .FirstOrDefaultAsync(h =>
                    h.Id == baseHeadId.Value &&
                    h.SubscriptionId == subscriptionId)
                ?? throw new KeyNotFoundException(
                    $"Base salary head with ID {baseHeadId} not found.");

            if (baseHead.HeadType != "Earning")
            {
                throw new InvalidOperationException(
                    "BaseHeadId must reference an Earning head. " +
                    "Deduction heads cannot be used as a calculation base.");
            }
        }
    }

    private async Task EnsureUniqueAsync(string headName, string headCode, int subscriptionId, int? excludeId = null)
    {
        var nameExists = await _salaryHeadRepository.Query()
            .AnyAsync(h =>
                h.HeadName == headName &&
                h.SubscriptionId == subscriptionId &&
                (excludeId == null || h.Id != excludeId));

        if (nameExists)
        {
            throw new InvalidOperationException($"A salary head named '{headName}' already exists.");
        }

        var codeExists = await _salaryHeadRepository.Query()
            .AnyAsync(h =>
                h.HeadCode == headCode &&
                h.SubscriptionId == subscriptionId &&
                (excludeId == null || h.Id != excludeId));

        if (codeExists)
        {
            throw new InvalidOperationException($"A salary head with code '{headCode}' already exists.");
        }
    }

    private static void ValidateHeadType(string headType)
    {
        if (!ValidHeadTypes.Contains(headType))
        {
            throw new InvalidOperationException(
                $"Invalid HeadType '{headType}'. Accepted: {string.Join(", ", ValidHeadTypes)}.");
        }
    }

    private static void ValidateCalculationMethod(string method)
    {
        if (!ValidMethods.Contains(method))
        {
            throw new InvalidOperationException(
                $"Invalid CalculationMethod '{method}'. Accepted: {string.Join(", ", ValidMethods)}.");
        }
    }

    private int GetSubscriptionId()
    {
        return _httpContextAccessor.HttpContext?.User.GetSubscriptionId()
            ?? throw new UnauthorizedAccessException("No authenticated user on the request.");
    }
}
