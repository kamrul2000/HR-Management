using AutoMapper;
using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.Employee;
using HRM.Core.DTOs.LoanApplication;
using HRM.Core.Entities;
using HRM.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Services;

public class LoanApplicationService : ILoanApplicationService
{
    private const long MaxAttachmentBytes = 10L * 1024 * 1024;
    private const string AttachmentRelativeDirectory = "uploads/loan-attachments";
    private const int MaxPageSize = 100;

    private static readonly string[] ValidLoanTypes =
        { "Personal", "Medical", "Education", "Housing", "Emergency" };

    private static readonly IReadOnlyDictionary<string, string[]> AllowedAttachmentTypes =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["application/pdf"] = new[] { ".pdf" },
            ["image/jpeg"] = new[] { ".jpg", ".jpeg" },
            ["image/png"] = new[] { ".png" },
            ["application/msword"] = new[] { ".doc" },
            ["application/vnd.openxmlformats-officedocument.wordprocessingml.document"] = new[] { ".docx" }
        };

    private readonly IRepository<LoanApplication> _loanApplicationRepository;
    private readonly IRepository<Employee> _employeeRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWebHostEnvironment _env;
    private readonly IMapper _mapper;

    public LoanApplicationService(
        IRepository<LoanApplication> loanApplicationRepository,
        IRepository<Employee> employeeRepository,
        IHttpContextAccessor httpContextAccessor,
        IWebHostEnvironment env,
        IMapper mapper)
    {
        _loanApplicationRepository = loanApplicationRepository;
        _employeeRepository = employeeRepository;
        _httpContextAccessor = httpContextAccessor;
        _env = env;
        _mapper = mapper;
    }

    public async Task<LoanApplicationResponseDto> CreateAsync(CreateLoanApplicationDto dto)
    {
        var subscriptionId = GetSubscriptionId();
        var employee = await ResolveEmployeeAsync(dto.EmployeeId, subscriptionId);

        ValidateLoanType(dto.LoanType);
        await EnsureNoActiveApplicationAsync(employee.Id, subscriptionId);

        var applicationNo = await GenerateApplicationNoAsync(subscriptionId);
        var now = DateTime.UtcNow;

        var application = new LoanApplication
        {
            ApplicationNo = applicationNo,
            EmployeeId = employee.Id,
            LoanType = dto.LoanType,
            RequestedAmount = dto.RequestedAmount,
            RequestedTenureMonths = dto.RequestedTenureMonths,
            Purpose = dto.Purpose,
            Status = "Pending",
            SubscriptionId = subscriptionId,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _loanApplicationRepository.AddAsync(application);

        return await LoadResponseAsync(application.Id, subscriptionId);
    }

    public async Task<LoanApplicationResponseDto> UploadAttachmentAsync(int id, IFormFile file)
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

        var application = await _loanApplicationRepository.Query()
            .FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new KeyNotFoundException($"Loan application with ID {id} not found.");

        if (application.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this loan application.");
        }

        if (application.Status != "Pending")
        {
            throw new InvalidOperationException(
                "Attachments can only be uploaded to pending loan applications.");
        }

        var webRootPath = ResolveWebRootPath();
        var absoluteDirectory = Path.Combine(webRootPath, AttachmentRelativeDirectory);
        Directory.CreateDirectory(absoluteDirectory);

        var fileName = $"loan_{application.Id}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{extension}";
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
        await _loanApplicationRepository.UpdateAsync(application);

        return await LoadResponseAsync(application.Id, subscriptionId);
    }

    public async Task<LoanApplicationResponseDto> GetByIdAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();
        return await LoadResponseAsync(id, subscriptionId);
    }

    public async Task<PagedResultDto<LoanApplicationResponseDto>> GetFilteredAsync(LoanApplicationFilterDto filter)
    {
        var subscriptionId = GetSubscriptionId();

        var pageSize = Math.Clamp(filter.PageSize, 1, MaxPageSize);
        var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;

        var query = BaseQuery(subscriptionId).AsNoTracking();

        if (filter.EmployeeId is int empId)
        {
            query = query.Where(l => l.EmployeeId == empId);
        }

        if (filter.BranchId is int brId)
        {
            query = query.Where(l => l.Employee.BranchId == brId);
        }

        if (!string.IsNullOrWhiteSpace(filter.LoanType))
        {
            var type = filter.LoanType.Trim();
            query = query.Where(l => l.LoanType == type);
        }

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            var status = filter.Status.Trim();
            query = query.Where(l => l.Status == status);
        }

        if (filter.FromDate is DateTime from)
        {
            query = query.Where(l => l.CreatedAt >= from);
        }

        if (filter.ToDate is DateTime to)
        {
            var toEnd = to.Date.AddDays(1);
            query = query.Where(l => l.CreatedAt < toEnd);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var baseUrl = ResolveBaseUrl();

        return new PagedResultDto<LoanApplicationResponseDto>
        {
            Items = items.Select(l => MapToResponseDto(l, baseUrl)).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<LoanApplicationResponseDto>> GetByEmployeeAsync(int employeeId)
    {
        var subscriptionId = GetSubscriptionId();
        await EnsureEmployeeOwnershipAsync(employeeId, subscriptionId);

        var items = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .Where(l => l.EmployeeId == employeeId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

        var baseUrl = ResolveBaseUrl();
        return items.Select(l => MapToResponseDto(l, baseUrl)).ToList();
    }

    public async Task<LoanApplicationResponseDto> CancelAsync(int id, CancelLoanApplicationDto dto)
    {
        var subscriptionId = GetSubscriptionId();

        var application = await _loanApplicationRepository.Query()
            .FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new KeyNotFoundException($"Loan application with ID {id} not found.");

        if (application.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this loan application.");
        }

        if (application.Status != "Pending" && application.Status != "Approved")
        {
            throw new InvalidOperationException(
                $"Loan application in status '{application.Status}' cannot be cancelled here. " +
                "Recommended cancellations are handled by the recommendation module; " +
                "Disbursed/Rejected/Cancelled are terminal.");
        }

        var now = DateTime.UtcNow;
        application.Status = "Cancelled";
        application.RejectionRemarks = dto.CancellationReason;
        application.RejectionDate = now;
        application.UpdatedAt = now;

        await _loanApplicationRepository.UpdateAsync(application);

        return await LoadResponseAsync(application.Id, subscriptionId);
    }

    public async Task DeleteAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();

        var application = await _loanApplicationRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Loan application with ID {id} not found.");

        if (application.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this loan application.");
        }

        if (application.Status != "Pending" && application.Status != "Cancelled")
        {
            throw new InvalidOperationException(
                $"Only pending or cancelled loan applications can be deleted. Current status: '{application.Status}'.");
        }

        if (!string.IsNullOrWhiteSpace(application.AttachmentPath))
        {
            DeleteAttachmentFromDisk(application.AttachmentPath);
        }

        await _loanApplicationRepository.DeleteAsync(application);
    }

    private IQueryable<LoanApplication> BaseQuery(int subscriptionId)
    {
        return _loanApplicationRepository
            .Query()
            .Include(l => l.Employee)
            .Where(l => l.SubscriptionId == subscriptionId);
    }

    private async Task<LoanApplicationResponseDto> LoadResponseAsync(int applicationId, int subscriptionId)
    {
        var application = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == applicationId);

        if (application is null)
        {
            var existsForOtherTenant = await _loanApplicationRepository.Query()
                .AnyAsync(l => l.Id == applicationId);

            if (existsForOtherTenant)
            {
                throw new UnauthorizedAccessException("Access denied to this loan application.");
            }

            throw new KeyNotFoundException($"Loan application with ID {applicationId} not found.");
        }

        return MapToResponseDto(application, ResolveBaseUrl());
    }

    private LoanApplicationResponseDto MapToResponseDto(LoanApplication l, string? baseUrl = null)
    {
        var dto = _mapper.Map<LoanApplicationResponseDto>(l);

        dto.LoanTypeLabel = l.LoanType switch
        {
            "Personal" => "Personal Loan",
            "Medical" => "Medical Loan",
            "Education" => "Education Loan",
            "Housing" => "Housing Loan",
            "Emergency" => "Emergency Loan",
            _ => l.LoanType
        };

        dto.RequestedAmountFormatted = l.RequestedAmount.ToString("N2");

        int years = l.RequestedTenureMonths / 12;
        int months = l.RequestedTenureMonths % 12;
        dto.TenureLabel = years > 0 && months > 0
            ? $"{l.RequestedTenureMonths} months ({years} year(s) {months} month(s))"
            : years > 0
                ? $"{l.RequestedTenureMonths} months ({years} year(s))"
                : $"{l.RequestedTenureMonths} month(s)";

        decimal estimated = l.RequestedTenureMonths > 0
            ? Math.Round(l.RequestedAmount / l.RequestedTenureMonths, 2)
            : 0m;
        dto.EstimatedMonthlyInstallment = estimated;
        dto.EstimatedMonthlyInstallmentFormatted = estimated.ToString("N2");

        dto.AttachmentUrl = !string.IsNullOrWhiteSpace(l.AttachmentPath) && !string.IsNullOrWhiteSpace(baseUrl)
            ? $"{baseUrl.TrimEnd('/')}/{l.AttachmentPath.TrimStart('/')}"
            : null;

        dto.StatusLabel = l.Status switch
        {
            "Pending" => "Pending Review",
            "Recommended" => "Supervisor Recommended",
            "Approved" => "HR Approved",
            "Rejected" => "Rejected",
            "Disbursed" => "Disbursed",
            "Cancelled" => "Cancelled",
            _ => l.Status
        };

        dto.RecommendationDateFormatted = l.RecommendationDate?.ToString("dd MMM yyyy");
        dto.RejectionDateFormatted = l.RejectionDate?.ToString("dd MMM yyyy");

        return dto;
    }

    private async Task EnsureNoActiveApplicationAsync(int employeeId, int subscriptionId)
    {
        var activeStatuses = new[] { "Pending", "Recommended", "Approved", "Disbursed" };

        var hasActive = await _loanApplicationRepository.Query()
            .AnyAsync(l =>
                l.EmployeeId == employeeId &&
                l.SubscriptionId == subscriptionId &&
                activeStatuses.Contains(l.Status));

        if (hasActive)
        {
            throw new InvalidOperationException(
                "This employee already has an active loan application or disbursed loan. " +
                "An employee may have only one active loan at a time.");
        }
    }

    private async Task<string> GenerateApplicationNoAsync(int subscriptionId)
    {
        int year = DateTime.UtcNow.Year;
        int count = await _loanApplicationRepository.Query()
            .CountAsync(l =>
                l.CreatedAt.Year == year &&
                l.SubscriptionId == subscriptionId);
        return $"LN-{year}-{(count + 1):D4}";
    }

    private static void ValidateLoanType(string loanType)
    {
        if (!ValidLoanTypes.Contains(loanType))
        {
            throw new InvalidOperationException(
                $"Invalid LoanType '{loanType}'. Accepted: {string.Join(", ", ValidLoanTypes)}.");
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
                "Loan applications can only be submitted for active employees.");
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
