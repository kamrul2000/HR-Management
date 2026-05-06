using AutoMapper;
using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.Employee;
using HRM.Core.DTOs.EmployeeSeparation;
using HRM.Core.Entities;
using HRM.Infrastructure.Data;
using HRM.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Services;

public class EmployeeSeparationService : IEmployeeSeparationService
{
    private const int MaxPageSize = 100;
    private const long MaxAttachmentBytes = 10L * 1024 * 1024;
    private const string AttachmentRelativeDirectory = "uploads/separations";

    private static readonly string[] ValidSeparationTypes =
        { "Resignation", "Termination", "Retirement", "Redundancy", "Death" };

    private static readonly IReadOnlyDictionary<string, string[]> AllowedAttachmentTypes =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["application/pdf"] = new[] { ".pdf" },
            ["image/jpeg"] = new[] { ".jpg", ".jpeg" },
            ["image/png"] = new[] { ".png" },
            ["application/msword"] = new[] { ".doc" },
            ["application/vnd.openxmlformats-officedocument.wordprocessingml.document"] = new[] { ".docx" }
        };

    private readonly IRepository<EmployeeSeparation> _separationRepository;
    private readonly IRepository<Employee> _employeeRepository;
    private readonly IRepository<SeparationReason> _reasonRepository;
    private readonly IRepository<GratuityCalculation> _gratuityRepository;
    private readonly IGratuityCalculationService _gratuityService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWebHostEnvironment _env;
    private readonly IMapper _mapper;
    private readonly AppDbContext _context;

    public EmployeeSeparationService(
        IRepository<EmployeeSeparation> separationRepository,
        IRepository<Employee> employeeRepository,
        IRepository<SeparationReason> reasonRepository,
        IRepository<GratuityCalculation> gratuityRepository,
        IGratuityCalculationService gratuityService,
        IHttpContextAccessor httpContextAccessor,
        IWebHostEnvironment env,
        IMapper mapper,
        AppDbContext context)
    {
        _separationRepository = separationRepository;
        _employeeRepository = employeeRepository;
        _reasonRepository = reasonRepository;
        _gratuityRepository = gratuityRepository;
        _gratuityService = gratuityService;
        _httpContextAccessor = httpContextAccessor;
        _env = env;
        _mapper = mapper;
        _context = context;
    }

    public async Task<SeparationResponseDto> CreateAsync(CreateSeparationDto dto)
    {
        var subscriptionId = GetSubscriptionId();

        var employee = await _employeeRepository.GetByIdAsync(dto.EmployeeId)
            ?? throw new KeyNotFoundException($"Employee with ID {dto.EmployeeId} not found.");

        if (employee.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this employee.");
        }

        if (employee.Status != "Active" || !employee.IsActive)
        {
            throw new InvalidOperationException(
                $"Cannot initiate separation for an employee with status '{employee.Status}'.");
        }

        ValidateSeparationType(dto.SeparationType);

        var reason = await _reasonRepository.GetByIdAsync(dto.SeparationReasonId)
            ?? throw new KeyNotFoundException($"Separation reason with ID {dto.SeparationReasonId} not found.");

        if (reason.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this separation reason.");
        }

        var applicationDate = dto.ApplicationDate.Date;
        var lastWorkingDate = dto.LastWorkingDate.Date;

        if (lastWorkingDate < applicationDate)
        {
            throw new InvalidOperationException(
                "LastWorkingDate cannot be before ApplicationDate.");
        }

        await EnsureNoActiveSeparationAsync(employee.Id, subscriptionId);

        decimal gratuityAmount = await TryGetFinalizedGratuityAmountAsync(employee.Id);

        var (actualNoticeDays, shortfall, totalSettlement) = ComputeSeparationFields(
            applicationDate, lastWorkingDate, dto.NoticePeriodDays,
            dto.NoticePeriodBuyout, gratuityAmount, dto.OtherSettlementAmount);

        var now = DateTime.UtcNow;
        var separation = new EmployeeSeparation
        {
            EmployeeId = employee.Id,
            SeparationReasonId = reason.Id,
            SeparationType = dto.SeparationType,
            ApplicationDate = applicationDate,
            LastWorkingDate = lastWorkingDate,
            NoticePeriodDays = dto.NoticePeriodDays,
            ActualNoticeDays = actualNoticeDays,
            NoticePeriodShortfall = shortfall,
            NoticePeriodBuyout = dto.NoticePeriodBuyout,
            GratuityAmount = gratuityAmount,
            OtherSettlementAmount = dto.OtherSettlementAmount,
            TotalSettlementAmount = totalSettlement,
            Remarks = dto.Remarks,
            Status = "Draft",
            SubscriptionId = subscriptionId,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _context.EmployeeSeparations.AddAsync(separation);
        await _context.SaveChangesAsync();

        return await LoadResponseAsync(separation.Id, subscriptionId);
    }

    public async Task<SeparationResponseDto> UploadAttachmentAsync(int id, IFormFile file)
    {
        var subscriptionId = GetSubscriptionId();

        if (file is null || file.Length == 0)
        {
            throw new InvalidOperationException("Attachment file is required.");
        }

        if (file.Length > MaxAttachmentBytes)
        {
            throw new InvalidOperationException("Attachment file size must not exceed 10 MB.");
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

        var separation = await _separationRepository.Query()
            .FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new KeyNotFoundException($"Separation with ID {id} not found.");

        if (separation.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this separation.");
        }

        if (separation.Status != "Draft")
        {
            throw new InvalidOperationException(
                $"Attachments can only be uploaded while the separation is in Draft status. Current status: '{separation.Status}'.");
        }

        var webRootPath = ResolveWebRootPath();
        var absoluteDirectory = Path.Combine(webRootPath, AttachmentRelativeDirectory);
        Directory.CreateDirectory(absoluteDirectory);

        var fileName = $"sep_{separation.Id}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{extension}";
        var absoluteFilePath = Path.Combine(absoluteDirectory, fileName);

        if (!string.IsNullOrWhiteSpace(separation.AttachmentPath))
        {
            DeleteAttachmentFromDisk(separation.AttachmentPath);
        }

        await using (var fileStream = new FileStream(absoluteFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await file.CopyToAsync(fileStream);
        }

        separation.AttachmentPath = $"{AttachmentRelativeDirectory}/{fileName}";
        separation.UpdatedAt = DateTime.UtcNow;
        await _separationRepository.UpdateAsync(separation);

        return await LoadResponseAsync(separation.Id, subscriptionId);
    }

    public async Task<SeparationResponseDto> GetByIdAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();
        return await LoadResponseAsync(id, subscriptionId);
    }

    public async Task<SeparationResponseDto> GetByEmployeeAsync(int employeeId)
    {
        var subscriptionId = GetSubscriptionId();
        await EnsureEmployeeOwnershipAsync(employeeId, subscriptionId);

        var separation = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .Where(s => s.EmployeeId == employeeId && s.Status != "Cancelled")
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException("No active separation found for this employee.");

        return MapToResponseDto(separation, ResolveBaseUrl());
    }

    public async Task<PagedResultDto<SeparationResponseDto>> GetFilteredAsync(SeparationFilterDto filter)
    {
        var subscriptionId = GetSubscriptionId();

        var pageSize = Math.Clamp(filter.PageSize, 1, MaxPageSize);
        var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;

        var query = BaseQuery(subscriptionId).AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.SeparationType))
        {
            var type = filter.SeparationType.Trim();
            query = query.Where(s => s.SeparationType == type);
        }

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            var status = filter.Status.Trim();
            query = query.Where(s => s.Status == status);
        }

        if (filter.BranchId is int branchId)
        {
            query = query.Where(s => s.Employee.BranchId == branchId);
        }

        if (filter.FromDate is DateTime from)
        {
            var fromDate = from.Date;
            query = query.Where(s => s.LastWorkingDate >= fromDate);
        }

        if (filter.ToDate is DateTime to)
        {
            var toDate = to.Date;
            query = query.Where(s => s.LastWorkingDate <= toDate);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(s => s.LastWorkingDate)
            .ThenByDescending(s => s.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var baseUrl = ResolveBaseUrl();

        return new PagedResultDto<SeparationResponseDto>
        {
            Items = items.Select(s => MapToResponseDto(s, baseUrl)).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<SeparationResponseDto> ApproveAsync(int id, ApproveSeparationDto dto)
    {
        var subscriptionId = GetSubscriptionId();
        var callerId = GetCallerId();

        var separation = await _separationRepository.Query()
            .FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new KeyNotFoundException($"Separation with ID {id} not found.");

        if (separation.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this separation.");
        }

        if (separation.Status != "Draft")
        {
            throw new InvalidOperationException(
                $"Only Draft separations can be approved. Current status: '{separation.Status}'.");
        }

        var now = DateTime.UtcNow;
        separation.Status = "Approved";
        separation.ApprovedById = callerId;
        separation.ApprovalDate = now;
        separation.ApprovalRemarks = dto.ApprovalRemarks;
        separation.UpdatedAt = now;

        await _context.SaveChangesAsync();

        return await LoadResponseAsync(separation.Id, subscriptionId);
    }

    public async Task<SeparationResponseDto> ProcessAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();

        var separation = await _separationRepository.Query()
            .FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new KeyNotFoundException($"Separation with ID {id} not found.");

        if (separation.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this separation.");
        }

        if (separation.Status != "Approved")
        {
            throw new InvalidOperationException(
                $"Only Approved separations can be processed. Current status: '{separation.Status}'.");
        }

        var employee = await _employeeRepository.GetByIdAsync(separation.EmployeeId)
            ?? throw new KeyNotFoundException(
                $"Employee with ID {separation.EmployeeId} not found.");

        // Refresh gratuity link — pull latest finalized amount and back-link the calculation row.
        try
        {
            var gratuityDto = await _gratuityService.GetByEmployeeAsync(separation.EmployeeId);

            if (gratuityDto.Status == "Finalized" && gratuityDto.IsEligible)
            {
                separation.GratuityAmount = gratuityDto.GratuityAmount;

                var calc = await _gratuityRepository.Query()
                    .FirstOrDefaultAsync(c => c.Id == gratuityDto.Id);

                if (calc is not null && calc.SubscriptionId == subscriptionId)
                {
                    calc.SeparationId = separation.Id;
                    calc.UpdatedAt = DateTime.UtcNow;
                }
            }
        }
        catch (KeyNotFoundException)
        {
            // No gratuity computed for this employee — leave amount as captured at draft time.
        }

        separation.TotalSettlementAmount = Math.Max(0m,
            separation.GratuityAmount +
            separation.OtherSettlementAmount -
            separation.NoticePeriodBuyout);
        separation.TotalSettlementAmount = Math.Round(separation.TotalSettlementAmount, 2);

        var now = DateTime.UtcNow;
        employee.Status = ResolveEmployeeStatus(separation.SeparationType);
        employee.IsActive = false;
        employee.UpdatedAt = now;

        separation.Status = "Processed";
        separation.ProcessedDate = now.Date;
        separation.UpdatedAt = now;

        await _context.SaveChangesAsync();

        return await LoadResponseAsync(separation.Id, subscriptionId);
    }

    public async Task<SeparationResponseDto> CancelAsync(int id, CancelSeparationDto dto)
    {
        var subscriptionId = GetSubscriptionId();

        if (string.IsNullOrWhiteSpace(dto.CancellationReason))
        {
            throw new InvalidOperationException("Cancellation reason is required.");
        }

        var separation = await _separationRepository.Query()
            .FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new KeyNotFoundException($"Separation with ID {id} not found.");

        if (separation.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this separation.");
        }

        if (separation.Status == "Processed" || separation.Status == "Cancelled")
        {
            throw new InvalidOperationException(
                $"Cannot cancel a separation in '{separation.Status}' status.");
        }

        var now = DateTime.UtcNow;
        separation.Status = "Cancelled";
        separation.Remarks = string.IsNullOrWhiteSpace(separation.Remarks)
            ? dto.CancellationReason
            : $"{separation.Remarks} | Cancelled: {dto.CancellationReason}";
        separation.UpdatedAt = now;

        await _context.SaveChangesAsync();

        return await LoadResponseAsync(separation.Id, subscriptionId);
    }

    private async Task<decimal> TryGetFinalizedGratuityAmountAsync(int employeeId)
    {
        try
        {
            var gratuity = await _gratuityService.GetByEmployeeAsync(employeeId);
            if (gratuity.Status == "Finalized" && gratuity.IsEligible)
            {
                return gratuity.GratuityAmount;
            }
        }
        catch (KeyNotFoundException)
        {
            // No gratuity computed yet — treat as zero.
        }

        return 0m;
    }

    private static (int actualNoticeDays, int shortfall, decimal totalSettlement)
        ComputeSeparationFields(
            DateTime applicationDate, DateTime lastWorkingDate,
            int noticePeriodDays, decimal noticePeriodBuyout,
            decimal gratuityAmount, decimal otherSettlement)
    {
        int actualNoticeDays = Math.Max(0, (lastWorkingDate.Date - applicationDate.Date).Days);
        int shortfall = Math.Max(0, noticePeriodDays - actualNoticeDays);
        decimal total = gratuityAmount + otherSettlement - noticePeriodBuyout;
        if (total < 0m) total = 0m;
        return (actualNoticeDays, shortfall, Math.Round(total, 2));
    }

    private static string ResolveEmployeeStatus(string separationType) =>
        separationType switch
        {
            "Resignation" => "Resigned",
            "Termination" => "Terminated",
            "Retirement" => "Retired",
            "Redundancy" => "Terminated",
            "Death" => "Inactive",
            _ => "Inactive"
        };

    private static void ValidateSeparationType(string separationType)
    {
        if (!ValidSeparationTypes.Contains(separationType))
        {
            throw new InvalidOperationException(
                $"Invalid SeparationType '{separationType}'. " +
                $"Accepted: {string.Join(", ", ValidSeparationTypes)}.");
        }
    }

    private async Task EnsureNoActiveSeparationAsync(
        int employeeId, int subscriptionId, int? excludeId = null)
    {
        var exists = await _separationRepository.Query()
            .AnyAsync(s =>
                s.EmployeeId == employeeId &&
                s.SubscriptionId == subscriptionId &&
                s.Status != "Cancelled" &&
                (excludeId == null || s.Id != excludeId));

        if (exists)
        {
            throw new InvalidOperationException(
                "An active separation record already exists for this employee.");
        }
    }

    private IQueryable<EmployeeSeparation> BaseQuery(int subscriptionId)
    {
        return _separationRepository.Query()
            .Include(s => s.Employee)
            .Include(s => s.SeparationReason)
            .Where(s => s.SubscriptionId == subscriptionId);
    }

    private async Task<SeparationResponseDto> LoadResponseAsync(int id, int subscriptionId)
    {
        var separation = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (separation is null)
        {
            var existsForOtherTenant = await _separationRepository.Query()
                .AnyAsync(s => s.Id == id);

            if (existsForOtherTenant)
            {
                throw new UnauthorizedAccessException("Access denied to this separation.");
            }

            throw new KeyNotFoundException($"Separation with ID {id} not found.");
        }

        return MapToResponseDto(separation, ResolveBaseUrl());
    }

    private SeparationResponseDto MapToResponseDto(EmployeeSeparation s, string? baseUrl = null)
    {
        var dto = _mapper.Map<SeparationResponseDto>(s);

        dto.EmployeeCode = s.Employee?.EmployeeCode ?? string.Empty;
        dto.EmployeeFullName = s.Employee?.FullName ?? string.Empty;
        dto.EmployeeJoiningDate = s.Employee?.JoiningDate ?? default;
        dto.EmployeeJoiningDateFormatted = s.Employee?.JoiningDate.ToString("dd MMM yyyy") ?? string.Empty;
        dto.SeparationReasonName = s.SeparationReason?.ReasonName ?? string.Empty;

        dto.SeparationTypeLabel = s.SeparationType switch
        {
            "Resignation" => "Voluntary Resignation",
            "Termination" => "Termination by Employer",
            "Retirement" => "Retirement",
            "Redundancy" => "Redundancy / Lay-off",
            "Death" => "Death in Service",
            _ => s.SeparationType
        };

        dto.ApplicationDateFormatted = s.ApplicationDate.ToString("dd MMM yyyy");
        dto.LastWorkingDateFormatted = s.LastWorkingDate.ToString("dd MMM yyyy");

        dto.NoticePeriodShortfallLabel = s.NoticePeriodShortfall > 0
            ? $"{s.NoticePeriodShortfall} day(s) shortfall"
            : "Notice period fulfilled";

        dto.NoticePeriodBuyoutFormatted = s.NoticePeriodBuyout.ToString("N2");
        dto.GratuityAmountFormatted = s.GratuityAmount.ToString("N2");
        dto.OtherSettlementAmountFormatted = s.OtherSettlementAmount.ToString("N2");
        dto.TotalSettlementAmountFormatted = s.TotalSettlementAmount.ToString("N2");

        dto.AttachmentUrl = s.AttachmentPath is not null && baseUrl is not null
            ? $"{baseUrl}/{s.AttachmentPath}"
            : null;

        dto.StatusLabel = s.Status switch
        {
            "Draft" => "Draft",
            "Approved" => "Approved",
            "Processed" => "Processed",
            "Cancelled" => "Cancelled",
            _ => s.Status
        };

        dto.ApprovalDateFormatted = s.ApprovalDate?.ToString("dd MMM yyyy");
        dto.ProcessedDateFormatted = s.ProcessedDate?.ToString("dd MMM yyyy");

        return dto;
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

    private int GetCallerId()
    {
        return _httpContextAccessor.HttpContext?.User.GetUserId()
            ?? throw new UnauthorizedAccessException("No authenticated user on the request.");
    }
}
