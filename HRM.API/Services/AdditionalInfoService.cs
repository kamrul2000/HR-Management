using AutoMapper;
using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.AdditionalInfo;
using HRM.Core.Entities;
using HRM.Infrastructure.Data;
using HRM.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Services;

public class AdditionalInfoService : IAdditionalInfoService
{
    private const long MaxAttachmentBytes = 5L * 1024 * 1024;
    private const string AttachmentRelativeDirectory = "uploads/employee-docs";

    private static readonly IReadOnlyDictionary<string, string[]> AllowedAttachmentTypes =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["application/pdf"] = new[] { ".pdf" },
            ["image/jpeg"] = new[] { ".jpg", ".jpeg" },
            ["image/png"] = new[] { ".png" }
        };

    private readonly IRepository<EmergencyContact> _contactRepository;
    private readonly IRepository<EmployeeEducation> _educationRepository;
    private readonly IRepository<EmployeeExperience> _experienceRepository;
    private readonly IRepository<Employee> _employeeRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWebHostEnvironment _env;
    private readonly IMapper _mapper;
    private readonly AppDbContext _context;

    public AdditionalInfoService(
        IRepository<EmergencyContact> contactRepository,
        IRepository<EmployeeEducation> educationRepository,
        IRepository<EmployeeExperience> experienceRepository,
        IRepository<Employee> employeeRepository,
        IHttpContextAccessor httpContextAccessor,
        IWebHostEnvironment env,
        IMapper mapper,
        AppDbContext context)
    {
        _contactRepository = contactRepository;
        _educationRepository = educationRepository;
        _experienceRepository = experienceRepository;
        _employeeRepository = employeeRepository;
        _httpContextAccessor = httpContextAccessor;
        _env = env;
        _mapper = mapper;
        _context = context;
    }

    // ───────────────────────────────────────────────────── Emergency Contacts

    public async Task<EmergencyContactDto> AddEmergencyContactAsync(int employeeId, CreateEmergencyContactDto dto)
    {
        var subscriptionId = GetSubscriptionId();
        await EnsureEmployeeOwnershipAsync(employeeId, subscriptionId);

        var now = DateTime.UtcNow;

        if (dto.IsPrimary)
        {
            await ClearPrimaryContactsAsync(employeeId, subscriptionId, now);
        }

        var contact = new EmergencyContact
        {
            EmployeeId = employeeId,
            ContactName = dto.ContactName,
            Relationship = dto.Relationship,
            Phone = dto.Phone,
            AlternatePhone = dto.AlternatePhone,
            Address = dto.Address,
            IsPrimary = dto.IsPrimary,
            SubscriptionId = subscriptionId,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _context.EmergencyContacts.AddAsync(contact);
        await _context.SaveChangesAsync();

        return MapContact(contact);
    }

    public async Task<IEnumerable<EmergencyContactDto>> GetEmergencyContactsAsync(int employeeId)
    {
        var subscriptionId = GetSubscriptionId();
        await EnsureEmployeeOwnershipAsync(employeeId, subscriptionId);

        var items = await _contactRepository.Query()
            .AsNoTracking()
            .Where(c => c.EmployeeId == employeeId && c.SubscriptionId == subscriptionId)
            .OrderByDescending(c => c.IsPrimary)
            .ThenBy(c => c.ContactName)
            .ToListAsync();

        return items.Select(MapContact).ToList();
    }

    public async Task<EmergencyContactDto> UpdateEmergencyContactAsync(int contactId, UpdateEmergencyContactDto dto)
    {
        var subscriptionId = GetSubscriptionId();

        var contact = await _contactRepository.Query()
            .FirstOrDefaultAsync(c => c.Id == contactId)
            ?? throw new KeyNotFoundException($"Emergency contact with ID {contactId} not found.");

        if (contact.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this emergency contact.");
        }

        var now = DateTime.UtcNow;

        if (dto.IsPrimary && !contact.IsPrimary)
        {
            await ClearPrimaryContactsAsync(contact.EmployeeId, subscriptionId, now, excludeId: contactId);
        }

        contact.ContactName = dto.ContactName;
        contact.Relationship = dto.Relationship;
        contact.Phone = dto.Phone;
        contact.AlternatePhone = dto.AlternatePhone;
        contact.Address = dto.Address;
        contact.IsPrimary = dto.IsPrimary;
        contact.UpdatedAt = now;

        await _context.SaveChangesAsync();

        return MapContact(contact);
    }

    public async Task DeleteEmergencyContactAsync(int contactId)
    {
        var subscriptionId = GetSubscriptionId();

        var contact = await _contactRepository.Query()
            .FirstOrDefaultAsync(c => c.Id == contactId)
            ?? throw new KeyNotFoundException($"Emergency contact with ID {contactId} not found.");

        if (contact.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this emergency contact.");
        }

        await _contactRepository.DeleteAsync(contact);
    }

    public async Task<EmergencyContactDto> SetPrimaryContactAsync(int contactId)
    {
        var subscriptionId = GetSubscriptionId();

        var contact = await _contactRepository.Query()
            .FirstOrDefaultAsync(c => c.Id == contactId)
            ?? throw new KeyNotFoundException($"Emergency contact with ID {contactId} not found.");

        if (contact.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this emergency contact.");
        }

        var now = DateTime.UtcNow;
        await ClearPrimaryContactsAsync(contact.EmployeeId, subscriptionId, now, excludeId: contactId);

        contact.IsPrimary = true;
        contact.UpdatedAt = now;

        await _context.SaveChangesAsync();

        return MapContact(contact);
    }

    private async Task ClearPrimaryContactsAsync(int employeeId, int subscriptionId, DateTime now, int? excludeId = null)
    {
        var others = await _contactRepository.Query()
            .Where(c =>
                c.EmployeeId == employeeId &&
                c.SubscriptionId == subscriptionId &&
                c.IsPrimary &&
                (excludeId == null || c.Id != excludeId))
            .ToListAsync();

        foreach (var other in others)
        {
            other.IsPrimary = false;
            other.UpdatedAt = now;
        }
    }

    // ───────────────────────────────────────────────────── Education

    public async Task<EducationDto> AddEducationAsync(int employeeId, CreateEducationDto dto)
    {
        var subscriptionId = GetSubscriptionId();
        await EnsureEmployeeOwnershipAsync(employeeId, subscriptionId);

        var now = DateTime.UtcNow;
        var education = new EmployeeEducation
        {
            EmployeeId = employeeId,
            Degree = dto.Degree,
            Institution = dto.Institution,
            PassingYear = dto.PassingYear,
            Result = dto.Result,
            MajorSubject = dto.MajorSubject,
            SubscriptionId = subscriptionId,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _context.EmployeeEducations.AddAsync(education);
        await _context.SaveChangesAsync();

        return MapEducation(education, ResolveBaseUrl());
    }

    public async Task<IEnumerable<EducationDto>> GetEducationAsync(int employeeId)
    {
        var subscriptionId = GetSubscriptionId();
        await EnsureEmployeeOwnershipAsync(employeeId, subscriptionId);

        var items = await _educationRepository.Query()
            .AsNoTracking()
            .Where(e => e.EmployeeId == employeeId && e.SubscriptionId == subscriptionId)
            .OrderByDescending(e => e.PassingYear)
            .ToListAsync();

        var baseUrl = ResolveBaseUrl();
        return items.Select(e => MapEducation(e, baseUrl)).ToList();
    }

    public async Task<EducationDto> UpdateEducationAsync(int educationId, UpdateEducationDto dto)
    {
        var subscriptionId = GetSubscriptionId();

        var education = await _educationRepository.Query()
            .FirstOrDefaultAsync(e => e.Id == educationId)
            ?? throw new KeyNotFoundException($"Education record with ID {educationId} not found.");

        if (education.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this education record.");
        }

        education.Degree = dto.Degree;
        education.Institution = dto.Institution;
        education.PassingYear = dto.PassingYear;
        education.Result = dto.Result;
        education.MajorSubject = dto.MajorSubject;
        education.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapEducation(education, ResolveBaseUrl());
    }

    public async Task<EducationDto> UploadEducationAttachmentAsync(int educationId, IFormFile file)
    {
        var subscriptionId = GetSubscriptionId();
        ValidateAttachment(file);

        var education = await _educationRepository.Query()
            .FirstOrDefaultAsync(e => e.Id == educationId)
            ?? throw new KeyNotFoundException($"Education record with ID {educationId} not found.");

        if (education.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this education record.");
        }

        var savedPath = await SaveAttachmentAsync(file, $"edu_{education.Id}", education.AttachmentPath);

        education.AttachmentPath = savedPath;
        education.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapEducation(education, ResolveBaseUrl());
    }

    public async Task DeleteEducationAsync(int educationId)
    {
        var subscriptionId = GetSubscriptionId();

        var education = await _educationRepository.Query()
            .FirstOrDefaultAsync(e => e.Id == educationId)
            ?? throw new KeyNotFoundException($"Education record with ID {educationId} not found.");

        if (education.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this education record.");
        }

        if (!string.IsNullOrWhiteSpace(education.AttachmentPath))
        {
            DeleteAttachmentFromDisk(education.AttachmentPath);
        }

        await _educationRepository.DeleteAsync(education);
    }

    // ───────────────────────────────────────────────────── Experience

    public async Task<ExperienceDto> AddExperienceAsync(int employeeId, CreateExperienceDto dto)
    {
        var subscriptionId = GetSubscriptionId();
        await EnsureEmployeeOwnershipAsync(employeeId, subscriptionId);
        ValidateExperienceDates(dto.FromDate, dto.ToDate, dto.IsCurrent);

        if (dto.IsCurrent)
        {
            await EnsureNoOtherCurrentExperienceAsync(employeeId, subscriptionId);
        }

        var now = DateTime.UtcNow;
        var experience = new EmployeeExperience
        {
            EmployeeId = employeeId,
            OrganizationName = dto.OrganizationName,
            Designation = dto.Designation,
            FromDate = dto.FromDate.Date,
            ToDate = dto.ToDate?.Date,
            IsCurrent = dto.IsCurrent,
            Responsibilities = dto.Responsibilities,
            ReasonForLeaving = dto.ReasonForLeaving,
            SubscriptionId = subscriptionId,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _context.EmployeeExperiences.AddAsync(experience);
        await _context.SaveChangesAsync();

        return MapExperience(experience, ResolveBaseUrl());
    }

    public async Task<IEnumerable<ExperienceDto>> GetExperienceAsync(int employeeId)
    {
        var subscriptionId = GetSubscriptionId();
        await EnsureEmployeeOwnershipAsync(employeeId, subscriptionId);

        var items = await _experienceRepository.Query()
            .AsNoTracking()
            .Where(e => e.EmployeeId == employeeId && e.SubscriptionId == subscriptionId)
            .OrderByDescending(e => e.IsCurrent)
            .ThenByDescending(e => e.FromDate)
            .ToListAsync();

        var baseUrl = ResolveBaseUrl();
        return items.Select(e => MapExperience(e, baseUrl)).ToList();
    }

    public async Task<ExperienceDto> UpdateExperienceAsync(int experienceId, UpdateExperienceDto dto)
    {
        var subscriptionId = GetSubscriptionId();

        var experience = await _experienceRepository.Query()
            .FirstOrDefaultAsync(e => e.Id == experienceId)
            ?? throw new KeyNotFoundException($"Experience record with ID {experienceId} not found.");

        if (experience.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this experience record.");
        }

        ValidateExperienceDates(dto.FromDate, dto.ToDate, dto.IsCurrent);

        if (dto.IsCurrent && !experience.IsCurrent)
        {
            await EnsureNoOtherCurrentExperienceAsync(experience.EmployeeId, subscriptionId, excludeId: experienceId);
        }

        experience.OrganizationName = dto.OrganizationName;
        experience.Designation = dto.Designation;
        experience.FromDate = dto.FromDate.Date;
        experience.ToDate = dto.ToDate?.Date;
        experience.IsCurrent = dto.IsCurrent;
        experience.Responsibilities = dto.Responsibilities;
        experience.ReasonForLeaving = dto.ReasonForLeaving;
        experience.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapExperience(experience, ResolveBaseUrl());
    }

    public async Task<ExperienceDto> UploadExperienceAttachmentAsync(int experienceId, IFormFile file)
    {
        var subscriptionId = GetSubscriptionId();
        ValidateAttachment(file);

        var experience = await _experienceRepository.Query()
            .FirstOrDefaultAsync(e => e.Id == experienceId)
            ?? throw new KeyNotFoundException($"Experience record with ID {experienceId} not found.");

        if (experience.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this experience record.");
        }

        var savedPath = await SaveAttachmentAsync(file, $"exp_{experience.Id}", experience.AttachmentPath);

        experience.AttachmentPath = savedPath;
        experience.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapExperience(experience, ResolveBaseUrl());
    }

    public async Task DeleteExperienceAsync(int experienceId)
    {
        var subscriptionId = GetSubscriptionId();

        var experience = await _experienceRepository.Query()
            .FirstOrDefaultAsync(e => e.Id == experienceId)
            ?? throw new KeyNotFoundException($"Experience record with ID {experienceId} not found.");

        if (experience.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this experience record.");
        }

        if (!string.IsNullOrWhiteSpace(experience.AttachmentPath))
        {
            DeleteAttachmentFromDisk(experience.AttachmentPath);
        }

        await _experienceRepository.DeleteAsync(experience);
    }

    private static void ValidateExperienceDates(DateTime fromDate, DateTime? toDate, bool isCurrent)
    {
        if (toDate.HasValue && toDate.Value.Date < fromDate.Date)
        {
            throw new InvalidOperationException("ToDate cannot be earlier than FromDate.");
        }

        if (isCurrent && toDate.HasValue)
        {
            throw new InvalidOperationException("Current experience must not have a ToDate.");
        }

        if (!isCurrent && !toDate.HasValue)
        {
            throw new InvalidOperationException("Past experience must have a ToDate.");
        }
    }

    private async Task EnsureNoOtherCurrentExperienceAsync(int employeeId, int subscriptionId, int? excludeId = null)
    {
        var conflict = await _experienceRepository.Query()
            .AnyAsync(e =>
                e.EmployeeId == employeeId &&
                e.SubscriptionId == subscriptionId &&
                e.IsCurrent &&
                (excludeId == null || e.Id != excludeId));

        if (conflict)
        {
            throw new InvalidOperationException(
                "Employee already has a current experience record. Mark the existing one as ended before adding another current experience.");
        }
    }

    // ───────────────────────────────────────────────────── Mapping

    private EmergencyContactDto MapContact(EmergencyContact c)
    {
        return _mapper.Map<EmergencyContactDto>(c);
    }

    private EducationDto MapEducation(EmployeeEducation e, string? baseUrl)
    {
        var dto = _mapper.Map<EducationDto>(e);
        dto.AttachmentUrl = e.AttachmentPath is not null && baseUrl is not null
            ? $"{baseUrl}/{e.AttachmentPath}"
            : null;
        return dto;
    }

    private ExperienceDto MapExperience(EmployeeExperience e, string? baseUrl)
    {
        var dto = _mapper.Map<ExperienceDto>(e);
        dto.AttachmentUrl = e.AttachmentPath is not null && baseUrl is not null
            ? $"{baseUrl}/{e.AttachmentPath}"
            : null;
        dto.FromDateFormatted = e.FromDate.ToString("dd MMM yyyy");
        dto.ToDateFormatted = e.ToDate?.ToString("dd MMM yyyy");
        dto.ServiceDurationLabel = ComputeDurationLabel(e.FromDate, e.ToDate, e.IsCurrent);
        return dto;
    }

    private static string ComputeDurationLabel(DateTime from, DateTime? to, bool isCurrent)
    {
        var end = to ?? (isCurrent ? DateTime.UtcNow.Date : from);
        if (end < from) end = from;

        int totalMonths = ((end.Year - from.Year) * 12) + end.Month - from.Month;
        if (totalMonths < 0) totalMonths = 0;

        int years = totalMonths / 12;
        int months = totalMonths % 12;

        if (years == 0 && months == 0) return "Less than 1 month";
        if (years > 0 && months > 0) return $"{years}y {months}m";
        if (years > 0) return $"{years} year{(years != 1 ? "s" : "")}";
        return $"{months} month{(months != 1 ? "s" : "")}";
    }

    // ───────────────────────────────────────────────────── Helpers

    private async Task EnsureEmployeeOwnershipAsync(int employeeId, int subscriptionId)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId)
            ?? throw new KeyNotFoundException($"Employee with ID {employeeId} not found.");

        if (employee.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this employee.");
        }
    }

    private static void ValidateAttachment(IFormFile file)
    {
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
            throw new InvalidOperationException("Attachment must be a PDF or image (JPEG/PNG).");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !permittedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Attachment file extension does not match the declared MIME type.");
        }
    }

    private async Task<string> SaveAttachmentAsync(IFormFile file, string filePrefix, string? oldRelativePath)
    {
        var webRootPath = ResolveWebRootPath();
        var absoluteDirectory = Path.Combine(webRootPath, AttachmentRelativeDirectory);
        Directory.CreateDirectory(absoluteDirectory);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{filePrefix}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{extension}";
        var absoluteFilePath = Path.Combine(absoluteDirectory, fileName);

        if (!string.IsNullOrWhiteSpace(oldRelativePath))
        {
            DeleteAttachmentFromDisk(oldRelativePath);
        }

        await using (var stream = new FileStream(absoluteFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await file.CopyToAsync(stream);
        }

        return $"{AttachmentRelativeDirectory}/{fileName}";
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
