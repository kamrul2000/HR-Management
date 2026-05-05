using AutoMapper;
using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.Employee;
using HRM.Core.DTOs.LeaveApplication;
using HRM.Core.Entities;
using HRM.Infrastructure.Data;
using HRM.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Services;

public class LeaveApplicationService : ILeaveApplicationService
{
    private const long MaxAttachmentBytes = 5L * 1024 * 1024;
    private const string AttachmentRelativeDirectory = "uploads/leave-attachments";
    private const int MaxPageSize = 100;

    private static readonly IReadOnlyDictionary<string, string[]> AllowedAttachmentTypes =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["application/pdf"] = new[] { ".pdf" },
            ["image/jpeg"] = new[] { ".jpg", ".jpeg" },
            ["image/png"] = new[] { ".png" },
            ["image/webp"] = new[] { ".webp" }
        };

    private readonly IRepository<LeaveApplication> _applicationRepository;
    private readonly IRepository<LeaveAllotment> _allotmentRepository;
    private readonly IRepository<Employee> _employeeRepository;
    private readonly IRepository<LeaveType> _leaveTypeRepository;
    private readonly IWorkingDayCalculator _workingDayCalculator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWebHostEnvironment _env;
    private readonly IMapper _mapper;
    private readonly AppDbContext _context;

    public LeaveApplicationService(
        IRepository<LeaveApplication> applicationRepository,
        IRepository<LeaveAllotment> allotmentRepository,
        IRepository<Employee> employeeRepository,
        IRepository<LeaveType> leaveTypeRepository,
        IWorkingDayCalculator workingDayCalculator,
        IHttpContextAccessor httpContextAccessor,
        IWebHostEnvironment env,
        IMapper mapper,
        AppDbContext context)
    {
        _applicationRepository = applicationRepository;
        _allotmentRepository = allotmentRepository;
        _employeeRepository = employeeRepository;
        _leaveTypeRepository = leaveTypeRepository;
        _workingDayCalculator = workingDayCalculator;
        _httpContextAccessor = httpContextAccessor;
        _env = env;
        _mapper = mapper;
        _context = context;
    }

    public async Task<LeaveApplicationResponseDto> CreateAsync(CreateLeaveApplicationDto dto)
    {
        var subscriptionId = GetSubscriptionId();
        var callerId = TryGetCallerId();

        var from = dto.FromDate.Date;
        var to = dto.ToDate.Date;

        if (to < from)
        {
            throw new InvalidOperationException("ToDate must be on or after FromDate.");
        }

        var employee = await ResolveEmployeeAsync(dto.EmployeeId, subscriptionId);
        var leaveType = await ResolveLeaveTypeAsync(dto.LeaveTypeId, subscriptionId);

        var totalDays = await ComputeEffectiveDaysAsync(from, to, employee.BranchId);
        if (totalDays <= 0)
        {
            throw new InvalidOperationException(
                "The requested date range contains no working days (entirely holidays or off days).");
        }

        EnforceLeaveTypePolicies(leaveType, employee, from, to, totalDays);

        var allotment = await _allotmentRepository.Query()
            .FirstOrDefaultAsync(a =>
                a.EmployeeId == employee.Id &&
                a.LeaveTypeId == leaveType.Id &&
                a.Year == from.Year &&
                a.SubscriptionId == subscriptionId &&
                a.IsActive)
            ?? throw new KeyNotFoundException(
                $"No active leave allotment found for this employee and leave type in {from.Year}.");

        if (allotment.RemainingDays < totalDays)
        {
            throw new InvalidOperationException(
                $"Insufficient leave balance. Available: {allotment.RemainingDays} day(s), Requested: {totalDays} day(s).");
        }

        await EnsureNoOverlapAsync(employee.Id, from, to, subscriptionId);

        var applicationNo = await GenerateApplicationNoAsync(from.Year, subscriptionId);
        var now = DateTime.UtcNow;

        var application = new LeaveApplication
        {
            ApplicationNo = applicationNo,
            EmployeeId = employee.Id,
            LeaveTypeId = leaveType.Id,
            LeaveAllotmentId = allotment.Id,
            FromDate = from,
            ToDate = to,
            TotalDays = totalDays,
            Reason = dto.Reason,
            Status = leaveType.RequiresApproval ? "Pending" : "Approved",
            SubscriptionId = subscriptionId,
            CreatedAt = now,
            UpdatedAt = now
        };

        if (!leaveType.RequiresApproval)
        {
            application.ApprovedById = callerId;
            application.ApprovalDate = now;
            allotment.UsedDays += totalDays;
            allotment.UpdatedAt = now;
        }

        await _context.LeaveApplications.AddAsync(application);
        await _context.SaveChangesAsync();

        return await LoadResponseAsync(application.Id, subscriptionId);
    }

    public async Task<LeaveApplicationResponseDto> UploadAttachmentAsync(int id, IFormFile file)
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
            throw new InvalidOperationException("Attachment must be a PDF, JPEG, PNG, or WebP file.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !permittedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Attachment file extension does not match the declared MIME type.");
        }

        var application = await _applicationRepository.Query()
            .FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new KeyNotFoundException($"Leave application with ID {id} not found.");

        if (application.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this leave application.");
        }

        if (application.Status != "Pending")
        {
            throw new InvalidOperationException("Attachments can only be uploaded to pending applications.");
        }

        var webRootPath = ResolveWebRootPath();
        var absoluteDirectory = Path.Combine(webRootPath, AttachmentRelativeDirectory);
        Directory.CreateDirectory(absoluteDirectory);

        var fileName = $"leave_{application.Id}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{extension}";
        var absoluteFilePath = Path.Combine(absoluteDirectory, fileName);

        if (!string.IsNullOrWhiteSpace(application.AttachmentPath))
        {
            DeleteAttachmentFromDisk(application.AttachmentPath);
        }

        await using (var fileStream = new FileStream(absoluteFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await file.CopyToAsync(fileStream);
        }

        application.AttachmentPath = $"{AttachmentRelativeDirectory}/{fileName}";
        application.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await LoadResponseAsync(application.Id, subscriptionId);
    }

    public async Task<LeaveApplicationResponseDto> GetByIdAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();
        return await LoadResponseAsync(id, subscriptionId);
    }

    public async Task<PagedResultDto<LeaveApplicationResponseDto>> GetFilteredAsync(LeaveApplicationFilterDto filter)
    {
        var subscriptionId = GetSubscriptionId();

        var pageSize = Math.Clamp(filter.PageSize, 1, MaxPageSize);
        var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;

        var query = BaseQuery(subscriptionId).AsNoTracking();

        if (filter.EmployeeId is int empId)
        {
            query = query.Where(a => a.EmployeeId == empId);
        }

        if (filter.LeaveTypeId is int ltId)
        {
            query = query.Where(a => a.LeaveTypeId == ltId);
        }

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            var status = filter.Status.Trim();
            query = query.Where(a => a.Status == status);
        }

        if (filter.FromDate is DateTime from)
        {
            var fromDate = from.Date;
            query = query.Where(a => a.FromDate >= fromDate);
        }

        if (filter.ToDate is DateTime to)
        {
            var toDate = to.Date;
            query = query.Where(a => a.ToDate <= toDate);
        }

        if (filter.Year is int year)
        {
            query = query.Where(a => a.FromDate.Year == year);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var baseUrl = ResolveBaseUrl();

        return new PagedResultDto<LeaveApplicationResponseDto>
        {
            Items = items.Select(a => MapToResponseDto(a, baseUrl)).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<LeaveApplicationResponseDto>> GetByEmployeeAsync(int employeeId, int? year = null)
    {
        var subscriptionId = GetSubscriptionId();
        await EnsureEmployeeOwnershipAsync(employeeId, subscriptionId);

        var query = BaseQuery(subscriptionId)
            .AsNoTracking()
            .Where(a => a.EmployeeId == employeeId);

        if (year.HasValue)
        {
            query = query.Where(a => a.FromDate.Year == year.Value);
        }

        var items = await query
            .OrderByDescending(a => a.FromDate)
            .ToListAsync();

        var baseUrl = ResolveBaseUrl();
        return items.Select(a => MapToResponseDto(a, baseUrl)).ToList();
    }

    public async Task<LeaveApplicationResponseDto> ApproveAsync(int id, ApproveLeaveDto dto)
    {
        var subscriptionId = GetSubscriptionId();
        var callerId = GetCallerId();

        var application = await BaseQuery(subscriptionId)
            .FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new KeyNotFoundException($"Leave application with ID {id} not found.");

        if (application.Status != "Pending")
        {
            throw new InvalidOperationException(
                $"Only pending applications can be approved. Current status: '{application.Status}'.");
        }

        var leaveType = application.LeaveType;
        if (leaveType.RequiresDocument && string.IsNullOrWhiteSpace(application.AttachmentPath))
        {
            throw new InvalidOperationException(
                "This leave type requires a supporting document before approval.");
        }

        var allotment = await _allotmentRepository.GetByIdAsync(application.LeaveAllotmentId)
            ?? throw new KeyNotFoundException($"Allotment with ID {application.LeaveAllotmentId} not found.");

        if (allotment.RemainingDays < application.TotalDays)
        {
            throw new InvalidOperationException(
                $"Insufficient leave balance at approval time. Available: {allotment.RemainingDays} day(s), Requested: {application.TotalDays} day(s).");
        }

        var now = DateTime.UtcNow;
        application.Status = "Approved";
        application.ApprovedById = callerId;
        application.ApprovalDate = now;
        application.ApprovalRemarks = dto.ApprovalRemarks;
        application.UpdatedAt = now;

        allotment.UsedDays += application.TotalDays;
        allotment.UpdatedAt = now;

        await _context.SaveChangesAsync();

        return await LoadResponseAsync(application.Id, subscriptionId);
    }

    public async Task<LeaveApplicationResponseDto> RejectAsync(int id, RejectLeaveDto dto)
    {
        var subscriptionId = GetSubscriptionId();
        var callerId = GetCallerId();

        var application = await _applicationRepository.Query()
            .FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new KeyNotFoundException($"Leave application with ID {id} not found.");

        if (application.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this leave application.");
        }

        if (application.Status != "Pending")
        {
            throw new InvalidOperationException(
                $"Only pending applications can be rejected. Current status: '{application.Status}'.");
        }

        var now = DateTime.UtcNow;
        application.Status = "Rejected";
        application.ApprovedById = callerId;
        application.ApprovalDate = now;
        application.ApprovalRemarks = dto.ApprovalRemarks;
        application.UpdatedAt = now;

        await _context.SaveChangesAsync();

        return await LoadResponseAsync(application.Id, subscriptionId);
    }

    public async Task<LeaveApplicationResponseDto> CancelAsync(int id, CancelLeaveDto dto)
    {
        var subscriptionId = GetSubscriptionId();

        var application = await _applicationRepository.Query()
            .FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new KeyNotFoundException($"Leave application with ID {id} not found.");

        if (application.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this leave application.");
        }

        if (application.Status != "Pending" && application.Status != "Approved")
        {
            throw new InvalidOperationException(
                $"Only pending or approved applications can be cancelled. Current status: '{application.Status}'.");
        }

        var now = DateTime.UtcNow;

        if (application.Status == "Approved")
        {
            var allotment = await _allotmentRepository.GetByIdAsync(application.LeaveAllotmentId)
                ?? throw new KeyNotFoundException($"Allotment with ID {application.LeaveAllotmentId} not found.");

            allotment.UsedDays = Math.Max(0m, allotment.UsedDays - application.TotalDays);
            allotment.UpdatedAt = now;
        }

        application.Status = "Cancelled";
        application.CancellationReason = dto.CancellationReason;
        application.UpdatedAt = now;

        await _context.SaveChangesAsync();

        return await LoadResponseAsync(application.Id, subscriptionId);
    }

    public async Task DeleteAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();

        var application = await _applicationRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Leave application with ID {id} not found.");

        if (application.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this leave application.");
        }

        if (application.Status == "Approved")
        {
            throw new InvalidOperationException(
                "Approved leave applications cannot be deleted. Cancel the application instead.");
        }

        if (!string.IsNullOrWhiteSpace(application.AttachmentPath))
        {
            DeleteAttachmentFromDisk(application.AttachmentPath);
        }

        await _applicationRepository.DeleteAsync(application);
    }

    private IQueryable<LeaveApplication> BaseQuery(int subscriptionId)
    {
        return _applicationRepository
            .Query()
            .Include(a => a.Employee)
            .Include(a => a.LeaveType)
            .Include(a => a.LeaveAllotment)
            .Where(a => a.SubscriptionId == subscriptionId);
    }

    private async Task<LeaveApplicationResponseDto> LoadResponseAsync(int applicationId, int subscriptionId)
    {
        var application = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == applicationId);

        if (application is null)
        {
            var existsForOtherTenant = await _applicationRepository.Query()
                .AnyAsync(a => a.Id == applicationId);

            if (existsForOtherTenant)
            {
                throw new UnauthorizedAccessException("Access denied to this leave application.");
            }

            throw new KeyNotFoundException($"Leave application with ID {applicationId} not found.");
        }

        return MapToResponseDto(application, ResolveBaseUrl());
    }

    private LeaveApplicationResponseDto MapToResponseDto(LeaveApplication a, string? baseUrl = null)
    {
        var dto = _mapper.Map<LeaveApplicationResponseDto>(a);

        dto.FromDateFormatted = a.FromDate.ToString("dd MMM yyyy");
        dto.ToDateFormatted = a.ToDate.ToString("dd MMM yyyy");
        dto.ApprovalDateFormatted = a.ApprovalDate?.ToString("dd MMM yyyy");

        dto.StatusLabel = a.Status switch
        {
            "Pending" => "Pending Approval",
            "Approved" => "Approved",
            "Rejected" => "Rejected",
            "Cancelled" => "Cancelled",
            _ => a.Status
        };

        dto.AttachmentUrl = !string.IsNullOrWhiteSpace(a.AttachmentPath) && !string.IsNullOrWhiteSpace(baseUrl)
            ? $"{baseUrl.TrimEnd('/')}/{a.AttachmentPath.TrimStart('/')}"
            : null;

        dto.RemainingBalanceAfter =
            a.LeaveAllotment.RemainingDays - (a.Status == "Approved" ? 0m : a.TotalDays);

        return dto;
    }

    private void EnforceLeaveTypePolicies(
        LeaveType leaveType, Employee employee,
        DateTime fromDate, DateTime toDate, decimal totalDays)
    {
        if (leaveType.GenderRestriction is not null &&
            !string.Equals(leaveType.GenderRestriction, employee.Gender, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"'{leaveType.Name}' is restricted to {leaveType.GenderRestriction} employees only.");
        }

        int noticeDays = (int)(fromDate.Date - DateTime.UtcNow.Date).TotalDays;
        if (noticeDays < leaveType.MinNoticeDays)
        {
            throw new InvalidOperationException(
                $"'{leaveType.Name}' requires at least {leaveType.MinNoticeDays} day(s) advance notice. " +
                $"You have provided {noticeDays} day(s).");
        }

        if (leaveType.MaxConsecutiveDays.HasValue && totalDays > leaveType.MaxConsecutiveDays.Value)
        {
            throw new InvalidOperationException(
                $"'{leaveType.Name}' allows a maximum of {leaveType.MaxConsecutiveDays} " +
                $"consecutive day(s) per application. Requested: {totalDays}.");
        }
    }

    private async Task EnsureNoOverlapAsync(int employeeId, DateTime from, DateTime to, int subscriptionId, int? excludeId = null)
    {
        var fromDate = from.Date;
        var toDate = to.Date;

        var overlap = await _applicationRepository.Query()
            .AnyAsync(a =>
                a.EmployeeId == employeeId &&
                a.SubscriptionId == subscriptionId &&
                a.Status != "Rejected" &&
                a.Status != "Cancelled" &&
                a.FromDate <= toDate &&
                a.ToDate >= fromDate &&
                (excludeId == null || a.Id != excludeId));

        if (overlap)
        {
            throw new InvalidOperationException(
                "This employee already has an active leave application overlapping the requested dates.");
        }
    }

    private async Task<decimal> ComputeEffectiveDaysAsync(DateTime from, DateTime to, int? branchId)
    {
        var workingDays = await _workingDayCalculator.CountWorkingDaysAsync(from, to, branchId);
        return workingDays;
    }

    private async Task<string> GenerateApplicationNoAsync(int year, int subscriptionId)
    {
        var count = await _applicationRepository.Query()
            .CountAsync(a => a.CreatedAt.Year == year && a.SubscriptionId == subscriptionId);
        return $"LA-{year}-{(count + 1):D4}";
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
            throw new InvalidOperationException("Leave applications can only be submitted for active employees.");
        }

        return employee;
    }

    private async Task<LeaveType> ResolveLeaveTypeAsync(int leaveTypeId, int subscriptionId)
    {
        var leaveType = await _leaveTypeRepository.GetByIdAsync(leaveTypeId)
            ?? throw new KeyNotFoundException($"LeaveType with ID {leaveTypeId} not found.");

        if (leaveType.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this leave type.");
        }

        if (!leaveType.IsActive)
        {
            throw new InvalidOperationException("Cannot apply for an inactive leave type.");
        }

        return leaveType;
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
        if (request is null)
        {
            return null;
        }

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

    private int? TryGetCallerId()
    {
        try { return GetCallerId(); }
        catch { return null; }
    }
}
