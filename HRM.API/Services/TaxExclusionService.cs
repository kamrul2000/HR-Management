using AutoMapper;
using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.TaxExclusion;
using HRM.Core.Entities;
using HRM.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Services;

public class TaxExclusionService : ITaxExclusionService
{
    private const long MaxAttachmentBytes = 5L * 1024 * 1024;
    private const string AttachmentRelativeDirectory = "uploads/tax-exclusions";

    private static readonly IReadOnlyDictionary<string, string[]> AllowedAttachmentTypes =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["application/pdf"] = new[] { ".pdf" },
            ["image/jpeg"] = new[] { ".jpg", ".jpeg" },
            ["image/png"] = new[] { ".png" },
            ["application/msword"] = new[] { ".doc" },
            ["application/vnd.openxmlformats-officedocument.wordprocessingml.document"] = new[] { ".docx" }
        };

    private readonly IRepository<TaxExclusion> _exclusionRepository;
    private readonly IRepository<Employee> _employeeRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWebHostEnvironment _env;
    private readonly IMapper _mapper;

    public TaxExclusionService(
        IRepository<TaxExclusion> exclusionRepository,
        IRepository<Employee> employeeRepository,
        IHttpContextAccessor httpContextAccessor,
        IWebHostEnvironment env,
        IMapper mapper)
    {
        _exclusionRepository = exclusionRepository;
        _employeeRepository = employeeRepository;
        _httpContextAccessor = httpContextAccessor;
        _env = env;
        _mapper = mapper;
    }

    public async Task<TaxExclusionResponseDto> CreateAsync(CreateTaxExclusionDto dto)
    {
        var subscriptionId = GetSubscriptionId();
        var employee = await ResolveEmployeeAsync(dto.EmployeeId, subscriptionId);

        ValidateExclusionType(dto.ExclusionType, dto.PartialExclusionAmount);

        var from = dto.EffectiveFrom.Date;
        var to = dto.EffectiveTo?.Date;

        if (to.HasValue && to.Value <= from)
        {
            throw new InvalidOperationException("EffectiveTo must be after EffectiveFrom.");
        }

        if (dto.ExclusionType == "Full")
        {
            await EnsureNoFullExemptionOverlapAsync(employee.Id, from, to, subscriptionId);
        }

        var now = DateTime.UtcNow;
        var exclusion = new TaxExclusion
        {
            EmployeeId = employee.Id,
            Reason = dto.Reason,
            ExclusionType = dto.ExclusionType,
            PartialExclusionAmount = dto.PartialExclusionAmount,
            EffectiveFrom = from,
            EffectiveTo = to,
            CertificateNo = dto.CertificateNo,
            IsActive = true,
            SubscriptionId = subscriptionId,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _exclusionRepository.AddAsync(exclusion);

        return await LoadResponseAsync(exclusion.Id, subscriptionId);
    }

    public async Task<TaxExclusionResponseDto> UploadAttachmentAsync(int id, IFormFile file)
    {
        var subscriptionId = GetSubscriptionId();

        if (file is null || file.Length == 0)
        {
            throw new InvalidOperationException("Attachment file is required.");
        }

        if (file.Length > MaxAttachmentBytes)
        {
            throw new InvalidOperationException("Attachment file size must not exceed 5 MB.");
        }

        if (!AllowedAttachmentTypes.TryGetValue(file.ContentType ?? string.Empty, out var permittedExtensions))
        {
            throw new InvalidOperationException(
                "Attachment must be a PDF, image (JPEG/PNG), or Word document (.doc/.docx).");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !permittedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Attachment file extension does not match the declared MIME type.");
        }

        var exclusion = await _exclusionRepository.Query()
            .FirstOrDefaultAsync(e => e.Id == id)
            ?? throw new KeyNotFoundException($"Tax exclusion with ID {id} not found.");

        if (exclusion.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this tax exclusion.");
        }

        var webRootPath = ResolveWebRootPath();
        var absoluteDirectory = Path.Combine(webRootPath, AttachmentRelativeDirectory);
        Directory.CreateDirectory(absoluteDirectory);

        var fileName = $"taxexcl_{exclusion.Id}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{extension}";
        var absoluteFilePath = Path.Combine(absoluteDirectory, fileName);

        if (!string.IsNullOrWhiteSpace(exclusion.AttachmentPath))
        {
            DeleteAttachmentFromDisk(exclusion.AttachmentPath);
        }

        await using (var fileStream = new FileStream(absoluteFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await file.CopyToAsync(fileStream);
        }

        exclusion.AttachmentPath = $"{AttachmentRelativeDirectory}/{fileName}";
        exclusion.UpdatedAt = DateTime.UtcNow;
        await _exclusionRepository.UpdateAsync(exclusion);

        return await LoadResponseAsync(exclusion.Id, subscriptionId);
    }

    public async Task<TaxExclusionResponseDto> GetByIdAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();
        return await LoadResponseAsync(id, subscriptionId);
    }

    public async Task<IEnumerable<TaxExclusionResponseDto>> GetByEmployeeAsync(int employeeId)
    {
        var subscriptionId = GetSubscriptionId();
        await EnsureEmployeeOwnershipAsync(employeeId, subscriptionId);

        var items = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .Where(e => e.EmployeeId == employeeId)
            .OrderByDescending(e => e.EffectiveFrom)
            .ToListAsync();

        var baseUrl = ResolveBaseUrl();
        return items.Select(e => MapToResponseDto(e, baseUrl)).ToList();
    }

    public async Task<IEnumerable<TaxExclusionResponseDto>> GetAllActiveAsync()
    {
        var subscriptionId = GetSubscriptionId();
        var today = DateTime.UtcNow.Date;

        var items = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .Where(e =>
                e.IsActive &&
                e.EffectiveFrom <= today &&
                (e.EffectiveTo == null || e.EffectiveTo >= today))
            .OrderBy(e => e.Employee.FullName)
            .ToListAsync();

        var baseUrl = ResolveBaseUrl();
        return items.Select(e => MapToResponseDto(e, baseUrl)).ToList();
    }

    public Task<TaxExclusionCheckDto> CheckExclusionAsync(int employeeId)
    {
        var subscriptionId = GetSubscriptionId();
        return CheckExclusionAsync(employeeId, subscriptionId);
    }

    public async Task<TaxExclusionCheckDto> CheckExclusionAsync(int employeeId, int subscriptionId)
    {
        var today = DateTime.UtcNow.Date;

        var matches = await _exclusionRepository.Query()
            .AsNoTracking()
            .Where(e =>
                e.EmployeeId == employeeId &&
                e.SubscriptionId == subscriptionId &&
                e.IsActive &&
                e.EffectiveFrom <= today &&
                (e.EffectiveTo == null || e.EffectiveTo >= today))
            .ToListAsync();

        if (matches.Count == 0)
        {
            return new TaxExclusionCheckDto { IsExcluded = false };
        }

        var full = matches.FirstOrDefault(m => m.ExclusionType == "Full");
        if (full is not null)
        {
            return new TaxExclusionCheckDto
            {
                IsExcluded = true,
                ExclusionType = "Full"
            };
        }

        var partial = matches.FirstOrDefault(m => m.ExclusionType == "Partial");
        if (partial is not null)
        {
            return new TaxExclusionCheckDto
            {
                IsExcluded = true,
                ExclusionType = "Partial",
                PartialExclusionAmount = partial.PartialExclusionAmount
            };
        }

        return new TaxExclusionCheckDto { IsExcluded = false };
    }

    public async Task<TaxExclusionResponseDto> UpdateAsync(int id, UpdateTaxExclusionDto dto)
    {
        var subscriptionId = GetSubscriptionId();

        var exclusion = await _exclusionRepository.Query()
            .FirstOrDefaultAsync(e => e.Id == id)
            ?? throw new KeyNotFoundException($"Tax exclusion with ID {id} not found.");

        if (exclusion.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this tax exclusion.");
        }

        if (exclusion.ExclusionType == "Full" && dto.PartialExclusionAmount.HasValue)
        {
            throw new InvalidOperationException(
                "PartialExclusionAmount must not be provided on a Full exemption record.");
        }

        if (exclusion.ExclusionType == "Partial" && !dto.PartialExclusionAmount.HasValue)
        {
            throw new InvalidOperationException(
                "PartialExclusionAmount is required for a Partial exemption record.");
        }

        var newTo = dto.EffectiveTo?.Date;
        if (newTo.HasValue && newTo.Value <= exclusion.EffectiveFrom)
        {
            throw new InvalidOperationException("EffectiveTo must be after EffectiveFrom.");
        }

        if (exclusion.ExclusionType == "Full" &&
            dto.IsActive &&
            newTo != exclusion.EffectiveTo)
        {
            await EnsureNoFullExemptionOverlapAsync(
                exclusion.EmployeeId, exclusion.EffectiveFrom, newTo, subscriptionId, excludeId: id);
        }

        exclusion.Reason = dto.Reason;
        exclusion.PartialExclusionAmount = dto.PartialExclusionAmount;
        exclusion.EffectiveTo = newTo;
        exclusion.CertificateNo = dto.CertificateNo;
        exclusion.IsActive = dto.IsActive;
        exclusion.UpdatedAt = DateTime.UtcNow;

        await _exclusionRepository.UpdateAsync(exclusion);

        return await LoadResponseAsync(exclusion.Id, subscriptionId);
    }

    public async Task DeleteAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();

        var exclusion = await _exclusionRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Tax exclusion with ID {id} not found.");

        if (exclusion.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this tax exclusion.");
        }

        if (!string.IsNullOrWhiteSpace(exclusion.AttachmentPath))
        {
            DeleteAttachmentFromDisk(exclusion.AttachmentPath);
        }

        await _exclusionRepository.DeleteAsync(exclusion);
    }

    private IQueryable<TaxExclusion> BaseQuery(int subscriptionId)
    {
        return _exclusionRepository
            .Query()
            .Include(e => e.Employee)
            .Where(e => e.SubscriptionId == subscriptionId);
    }

    private async Task<TaxExclusionResponseDto> LoadResponseAsync(int exclusionId, int subscriptionId)
    {
        var exclusion = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == exclusionId);

        if (exclusion is null)
        {
            var existsForOtherTenant = await _exclusionRepository.Query()
                .AnyAsync(e => e.Id == exclusionId);

            if (existsForOtherTenant)
            {
                throw new UnauthorizedAccessException("Access denied to this tax exclusion.");
            }

            throw new KeyNotFoundException($"Tax exclusion with ID {exclusionId} not found.");
        }

        return MapToResponseDto(exclusion, ResolveBaseUrl());
    }

    private TaxExclusionResponseDto MapToResponseDto(TaxExclusion e, string? baseUrl = null)
    {
        var dto = _mapper.Map<TaxExclusionResponseDto>(e);

        dto.EmployeeCode = e.Employee?.EmployeeCode ?? string.Empty;
        dto.EmployeeFullName = e.Employee?.FullName ?? string.Empty;

        dto.ExclusionTypeLabel = e.ExclusionType switch
        {
            "Full" => "Full Tax Exemption",
            "Partial" => "Partial Tax Exemption",
            _ => e.ExclusionType
        };

        dto.PartialExclusionAmountFormatted = e.PartialExclusionAmount.HasValue
            ? e.PartialExclusionAmount.Value.ToString("N2")
            : null;

        dto.EffectiveFromFormatted = e.EffectiveFrom.ToString("dd MMM yyyy");
        dto.EffectiveToFormatted = e.EffectiveTo?.ToString("dd MMM yyyy");
        dto.IsIndefinite = !e.EffectiveTo.HasValue;
        dto.IsCurrentlyEffective = IsCurrentlyEffective(e);

        dto.AttachmentUrl = !string.IsNullOrWhiteSpace(e.AttachmentPath) && !string.IsNullOrWhiteSpace(baseUrl)
            ? $"{baseUrl.TrimEnd('/')}/{e.AttachmentPath.TrimStart('/')}"
            : null;

        return dto;
    }

    private async Task EnsureNoFullExemptionOverlapAsync(
        int employeeId, DateTime from, DateTime? to, int subscriptionId, int? excludeId = null)
    {
        var existing = await _exclusionRepository.Query()
            .Where(e =>
                e.EmployeeId == employeeId &&
                e.SubscriptionId == subscriptionId &&
                e.ExclusionType == "Full" &&
                e.IsActive &&
                (excludeId == null || e.Id != excludeId))
            .ToListAsync();

        var requestedFrom = from.Date;
        var requestedTo = to?.Date ?? DateTime.MaxValue.Date;

        foreach (var ex in existing)
        {
            var existingFrom = ex.EffectiveFrom.Date;
            var existingTo = ex.EffectiveTo?.Date ?? DateTime.MaxValue.Date;

            bool overlaps = existingFrom <= requestedTo && existingTo >= requestedFrom;
            if (overlaps)
            {
                var existingToLabel = ex.EffectiveTo.HasValue
                    ? ex.EffectiveTo.Value.ToString("dd MMM yyyy")
                    : "indefinite";
                throw new InvalidOperationException(
                    $"An active Full tax exemption already exists for this employee from " +
                    $"{ex.EffectiveFrom:dd MMM yyyy} to {existingToLabel}. " +
                    "Full exemptions cannot overlap.");
            }
        }
    }

    private static bool IsCurrentlyEffective(TaxExclusion e)
    {
        var today = DateTime.UtcNow.Date;
        return e.IsActive &&
               e.EffectiveFrom.Date <= today &&
               (!e.EffectiveTo.HasValue || e.EffectiveTo.Value.Date >= today);
    }

    private static void ValidateExclusionType(string exclusionType, decimal? partialAmount)
    {
        if (exclusionType != "Full" && exclusionType != "Partial")
        {
            throw new InvalidOperationException("ExclusionType must be 'Full' or 'Partial'.");
        }

        if (exclusionType == "Partial" && !partialAmount.HasValue)
        {
            throw new InvalidOperationException(
                "PartialExclusionAmount is required when ExclusionType is 'Partial'.");
        }

        if (exclusionType == "Full" && partialAmount.HasValue)
        {
            throw new InvalidOperationException(
                "PartialExclusionAmount must not be provided when ExclusionType is 'Full'.");
        }
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
                "Tax exclusions can only be created for active employees.");
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

    private string? ResolveBaseUrl()
    {
        var request = _httpContextAccessor.HttpContext?.Request;
        if (request is null) return null;
        return $"{request.Scheme}://{request.Host}{request.PathBase}";
    }

    private string ResolveWebRootPath()
    {
        var path = _env.WebRootPath;
        if (string.IsNullOrWhiteSpace(path))
        {
            path = Path.Combine(_env.ContentRootPath, "wwwroot");
        }
        Directory.CreateDirectory(path);
        return path;
    }

    private void DeleteAttachmentFromDisk(string relativePath)
    {
        var webRootPath = ResolveWebRootPath();
        var absolute = Path.Combine(webRootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(absolute))
        {
            File.Delete(absolute);
        }
    }

    private int GetSubscriptionId()
    {
        return _httpContextAccessor.HttpContext?.User.GetSubscriptionId()
            ?? throw new UnauthorizedAccessException("No authenticated user on the request.");
    }
}
