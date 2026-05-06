using AutoMapper;
using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.TaxSlab;
using HRM.Core.Entities;
using HRM.Infrastructure.Data;
using HRM.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Services;

public class TaxSlabService : ITaxSlabService
{
    private readonly IRepository<TaxSlabConfig> _configRepository;
    private readonly IRepository<TaxSlab> _slabRepository;
    private readonly IRepository<SalaryCalculation> _salaryCalculationRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;
    private readonly AppDbContext _context;

    public TaxSlabService(
        IRepository<TaxSlabConfig> configRepository,
        IRepository<TaxSlab> slabRepository,
        IRepository<SalaryCalculation> salaryCalculationRepository,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper,
        AppDbContext context)
    {
        _configRepository = configRepository;
        _slabRepository = slabRepository;
        _salaryCalculationRepository = salaryCalculationRepository;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
        _context = context;
    }

    public async Task<TaxSlabConfigResponseDto> CreateAsync(CreateTaxSlabConfigDto dto)
    {
        var subscriptionId = GetSubscriptionId();

        if (dto.EndDate <= dto.StartDate)
        {
            throw new InvalidOperationException("EndDate must be after StartDate.");
        }

        var fiscalYear = dto.FiscalYear.Trim();
        var duplicate = await _configRepository.Query()
            .AnyAsync(c =>
                c.FiscalYear == fiscalYear &&
                c.SubscriptionId == subscriptionId);

        if (duplicate)
        {
            throw new InvalidOperationException(
                $"A tax slab configuration for fiscal year '{fiscalYear}' already exists.");
        }

        ValidateSlabs(dto.Slabs);

        var now = DateTime.UtcNow;
        var config = new TaxSlabConfig
        {
            FiscalYear = fiscalYear,
            StartDate = dto.StartDate.Date,
            EndDate = dto.EndDate.Date,
            TaxFreeThreshold = dto.TaxFreeThreshold,
            Description = dto.Description,
            IsActive = true,
            SubscriptionId = subscriptionId,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _context.TaxSlabConfigs.AddAsync(config);
        await _context.SaveChangesAsync();

        foreach (var slabDto in dto.Slabs)
        {
            var slab = new TaxSlab
            {
                TaxSlabConfigId = config.Id,
                SlabOrder = slabDto.SlabOrder,
                MinAmount = slabDto.MinAmount,
                MaxAmount = slabDto.MaxAmount,
                TaxRate = slabDto.TaxRate,
                SubscriptionId = subscriptionId,
                CreatedAt = now,
                UpdatedAt = now
            };
            await _context.TaxSlabs.AddAsync(slab);
        }

        await _context.SaveChangesAsync();

        return await LoadResponseAsync(config.Id, subscriptionId);
    }

    public async Task<TaxSlabConfigResponseDto> GetByIdAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();
        return await LoadResponseAsync(id, subscriptionId);
    }

    public async Task<TaxSlabConfigResponseDto> GetActiveAsync()
    {
        var subscriptionId = GetSubscriptionId();

        var config = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.IsActive)
            ?? throw new KeyNotFoundException(
                "No active tax slab configuration found. Please create one for the current fiscal year.");

        return MapToResponseDto(config);
    }

    public async Task<TaxSlabConfigResponseDto> GetByFiscalYearAsync(string fiscalYear)
    {
        var subscriptionId = GetSubscriptionId();

        var config = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.FiscalYear == fiscalYear)
            ?? throw new KeyNotFoundException(
                $"No tax slab configuration found for fiscal year '{fiscalYear}'.");

        return MapToResponseDto(config);
    }

    public async Task<IEnumerable<TaxSlabConfigResponseDto>> GetAllAsync()
    {
        var subscriptionId = GetSubscriptionId();

        var configs = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .OrderByDescending(c => c.StartDate)
            .ToListAsync();

        return configs.Select(MapToResponseDto).ToList();
    }

    public async Task<TaxSlabConfigResponseDto> UpdateAsync(int id, UpdateTaxSlabConfigDto dto)
    {
        var subscriptionId = GetSubscriptionId();

        var config = await _configRepository.Query()
            .Include(c => c.Slabs)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException($"Tax slab configuration with ID {id} not found.");

        if (config.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this tax configuration.");
        }

        ValidateSlabs(dto.Slabs);

        var now = DateTime.UtcNow;

        if (dto.IsActive && !config.IsActive)
        {
            var others = await _configRepository.Query()
                .Where(c => c.SubscriptionId == subscriptionId && c.Id != id && c.IsActive)
                .ToListAsync();
            foreach (var other in others)
            {
                other.IsActive = false;
                other.UpdatedAt = now;
            }
        }

        config.TaxFreeThreshold = dto.TaxFreeThreshold;
        config.Description = dto.Description;
        config.IsActive = dto.IsActive;
        config.UpdatedAt = now;

        var existingSlabs = config.Slabs.ToList();
        foreach (var slab in existingSlabs)
        {
            _context.TaxSlabs.Remove(slab);
        }

        foreach (var slabDto in dto.Slabs)
        {
            var slab = new TaxSlab
            {
                TaxSlabConfigId = config.Id,
                SlabOrder = slabDto.SlabOrder,
                MinAmount = slabDto.MinAmount,
                MaxAmount = slabDto.MaxAmount,
                TaxRate = slabDto.TaxRate,
                SubscriptionId = subscriptionId,
                CreatedAt = now,
                UpdatedAt = now
            };
            await _context.TaxSlabs.AddAsync(slab);
        }

        await _context.SaveChangesAsync();

        return await LoadResponseAsync(config.Id, subscriptionId);
    }

    public async Task<TaxComputationResultDto> ComputeTaxAsync(ComputeTaxDto dto)
    {
        var subscriptionId = GetSubscriptionId();

        TaxSlabConfig? config;
        if (!string.IsNullOrWhiteSpace(dto.FiscalYear))
        {
            var fiscalYear = dto.FiscalYear.Trim();
            config = await BaseQuery(subscriptionId)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.FiscalYear == fiscalYear)
                ?? throw new KeyNotFoundException(
                    $"No tax slab configuration found for fiscal year '{fiscalYear}'.");
        }
        else
        {
            config = await BaseQuery(subscriptionId)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.IsActive)
                ?? throw new KeyNotFoundException(
                    "No active tax slab configuration found. Please create one for the current fiscal year.");
        }

        return ComputeTax(dto.AnnualIncome, config);
    }

    public async Task DeleteAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();

        var config = await _configRepository.Query()
            .Include(c => c.Slabs)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException($"Tax slab configuration with ID {id} not found.");

        if (config.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this tax configuration.");
        }

        var fiscalStart = config.StartDate;
        var fiscalEnd = config.EndDate;
        var hasUsage = await _salaryCalculationRepository.Query()
            .AnyAsync(s =>
                s.SubscriptionId == subscriptionId &&
                s.TaxDeduction > 0m &&
                ((s.Year * 100 + s.Month) >= (fiscalStart.Year * 100 + fiscalStart.Month)) &&
                ((s.Year * 100 + s.Month) <= (fiscalEnd.Year * 100 + fiscalEnd.Month)));

        if (hasUsage)
        {
            throw new InvalidOperationException(
                "Cannot delete a tax configuration that has been used in salary calculations.");
        }

        foreach (var slab in config.Slabs.ToList())
        {
            _context.TaxSlabs.Remove(slab);
        }

        _context.TaxSlabConfigs.Remove(config);
        await _context.SaveChangesAsync();
    }

    public async Task<TaxSlabConfig?> GetActiveConfigAsync(int subscriptionId)
    {
        return await _configRepository.Query()
            .AsNoTracking()
            .Include(c => c.Slabs)
            .FirstOrDefaultAsync(c =>
                c.SubscriptionId == subscriptionId &&
                c.IsActive);
    }

    public static TaxComputationResultDto ComputeTax(decimal annualIncome, TaxSlabConfig config)
    {
        var result = new TaxComputationResultDto
        {
            AnnualIncome = annualIncome,
            AnnualIncomeFormatted = annualIncome.ToString("N2"),
            TaxFreeThreshold = config.TaxFreeThreshold,
            FiscalYear = config.FiscalYear
        };

        if (annualIncome <= config.TaxFreeThreshold)
        {
            result.TaxableIncome = 0m;
            result.AnnualTax = 0m;
            result.MonthlyTax = 0m;
            result.EffectiveTaxRate = 0m;
            result.AnnualTaxFormatted = "0.00";
            result.MonthlyTaxFormatted = "0.00";
            result.EffectiveTaxRateLabel = "0.00%";
            return result;
        }

        decimal taxableIncome = annualIncome - config.TaxFreeThreshold;
        result.TaxableIncome = taxableIncome;

        decimal annualTax = 0m;
        decimal remaining = taxableIncome;

        var slabs = config.Slabs.OrderBy(s => s.SlabOrder).ToList();
        var breakdown = new List<TaxSlabBreakdownDto>();

        foreach (var slab in slabs)
        {
            if (remaining <= 0m) break;

            decimal bandWidth = slab.MaxAmount.HasValue
                ? slab.MaxAmount.Value - slab.MinAmount
                : remaining;

            decimal amountInBand = Math.Min(remaining, bandWidth);
            decimal taxForBand = Math.Round(amountInBand * slab.TaxRate / 100m, 2);

            annualTax += taxForBand;
            remaining -= amountInBand;

            string rangeLabel = slab.MaxAmount.HasValue
                ? $"{slab.MinAmount:N0} – {slab.MaxAmount.Value:N0} @ {slab.TaxRate}%"
                : $"{slab.MinAmount:N0} and above @ {slab.TaxRate}%";

            breakdown.Add(new TaxSlabBreakdownDto
            {
                SlabOrder = slab.SlabOrder,
                RangeLabel = rangeLabel,
                AmountInBand = amountInBand,
                TaxRate = slab.TaxRate,
                TaxAmount = taxForBand,
                TaxAmountFormatted = taxForBand.ToString("N2")
            });
        }

        decimal monthlyTax = Math.Round(annualTax / 12m, 2);
        decimal effectiveRate = annualIncome > 0m
            ? Math.Round(annualTax / annualIncome * 100m, 4)
            : 0m;

        result.AnnualTax = annualTax;
        result.AnnualTaxFormatted = annualTax.ToString("N2");
        result.MonthlyTax = monthlyTax;
        result.MonthlyTaxFormatted = monthlyTax.ToString("N2");
        result.EffectiveTaxRate = effectiveRate;
        result.EffectiveTaxRateLabel = $"{effectiveRate:F2}%";
        result.SlabBreakdown = breakdown;

        return result;
    }

    private IQueryable<TaxSlabConfig> BaseQuery(int subscriptionId)
    {
        return _configRepository
            .Query()
            .Include(c => c.Slabs)
            .Where(c => c.SubscriptionId == subscriptionId);
    }

    private async Task<TaxSlabConfigResponseDto> LoadResponseAsync(int configId, int subscriptionId)
    {
        var config = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == configId);

        if (config is null)
        {
            var existsForOtherTenant = await _configRepository.Query()
                .AnyAsync(c => c.Id == configId);

            if (existsForOtherTenant)
            {
                throw new UnauthorizedAccessException("Access denied to this tax configuration.");
            }

            throw new KeyNotFoundException($"Tax slab configuration with ID {configId} not found.");
        }

        return MapToResponseDto(config);
    }

    private TaxSlabConfigResponseDto MapToResponseDto(TaxSlabConfig c)
    {
        var dto = _mapper.Map<TaxSlabConfigResponseDto>(c);

        dto.StartDateFormatted = c.StartDate.ToString("dd MMM yyyy");
        dto.EndDateFormatted = c.EndDate.ToString("dd MMM yyyy");
        dto.TaxFreeThresholdFormatted = c.TaxFreeThreshold.ToString("N2");
        dto.SlabCount = c.Slabs.Count;

        dto.Slabs = c.Slabs
            .OrderBy(s => s.SlabOrder)
            .Select(s => new TaxSlabResponseDto
            {
                Id = s.Id,
                SlabOrder = s.SlabOrder,
                MinAmount = s.MinAmount,
                MinAmountFormatted = s.MinAmount.ToString("N0"),
                MaxAmount = s.MaxAmount,
                MaxAmountFormatted = s.MaxAmount.HasValue
                    ? s.MaxAmount.Value.ToString("N0")
                    : "and above",
                TaxRate = s.TaxRate,
                TaxRateLabel = $"{s.TaxRate:F2}%",
                RangeLabel = s.MaxAmount.HasValue
                    ? $"{s.MinAmount:N0} – {s.MaxAmount.Value:N0} @ {s.TaxRate}%"
                    : $"{s.MinAmount:N0} and above @ {s.TaxRate}%"
            })
            .ToList();

        return dto;
    }

    private static void ValidateSlabs(List<TaxSlabDto> slabs)
    {
        if (slabs.Count == 0)
        {
            throw new InvalidOperationException("At least one tax slab is required.");
        }

        var duplicateOrders = slabs
            .GroupBy(s => s.SlabOrder)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateOrders.Count > 0)
        {
            throw new InvalidOperationException(
                $"Duplicate SlabOrder values: {string.Join(", ", duplicateOrders)}.");
        }

        var ordered = slabs.OrderBy(s => s.SlabOrder).ToList();

        for (int i = 0; i < ordered.Count - 1; i++)
        {
            if (!ordered[i].MaxAmount.HasValue)
            {
                throw new InvalidOperationException(
                    $"Only the last slab (highest SlabOrder) may have a null MaxAmount. " +
                    $"Slab {ordered[i].SlabOrder} is not the last.");
            }
        }

        for (int i = 1; i < ordered.Count; i++)
        {
            if (ordered[i].MinAmount != ordered[i - 1].MaxAmount)
            {
                throw new InvalidOperationException(
                    $"Slab {ordered[i].SlabOrder} MinAmount ({ordered[i].MinAmount:N0}) " +
                    $"must equal the previous slab's MaxAmount ({ordered[i - 1].MaxAmount:N0}). " +
                    "Tax bands must be contiguous.");
            }
        }

        foreach (var slab in ordered.Where(s => s.MaxAmount.HasValue))
        {
            if (slab.MinAmount >= slab.MaxAmount!.Value)
            {
                throw new InvalidOperationException(
                    $"Slab {slab.SlabOrder}: MinAmount ({slab.MinAmount:N0}) must be " +
                    $"less than MaxAmount ({slab.MaxAmount:N0}).");
            }
        }
    }

    private int GetSubscriptionId()
    {
        return _httpContextAccessor.HttpContext?.User.GetSubscriptionId()
            ?? throw new UnauthorizedAccessException("No authenticated user on the request.");
    }
}
