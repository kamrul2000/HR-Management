using AutoMapper;
using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.Company;
using HRM.Core.Entities;
using HRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Services;

public class CompanyService : ICompanyService
{
    private const long MaxLogoBytes = 2L * 1024 * 1024;
    private const string LogoRelativeDirectory = "uploads/company-logos";

    private static readonly IReadOnlyDictionary<string, string[]> AllowedImageTypes =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["image/jpeg"] = new[] { ".jpg", ".jpeg" },
            ["image/png"] = new[] { ".png" },
            ["image/webp"] = new[] { ".webp" }
        };

    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWebHostEnvironment _env;
    private readonly IMapper _mapper;

    public CompanyService(
        AppDbContext context,
        IHttpContextAccessor httpContextAccessor,
        IWebHostEnvironment env,
        IMapper mapper)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _env = env;
        _mapper = mapper;
    }

    public async Task<CompanyResponseDto> CreateAsync(CreateCompanyDto dto)
    {
        var subscriptionId = CurrentSubscriptionId();
        var now = DateTime.UtcNow;

        var company = _mapper.Map<Company>(dto);
        company.SubscriptionId = subscriptionId;
        company.IsActive = true;
        company.CreatedAt = now;
        company.UpdatedAt = now;

        await _context.Companies.AddAsync(company);
        await _context.SaveChangesAsync();

        return ToResponse(company);
    }

    public async Task<CompanyResponseDto> GetByIdAsync(int id)
    {
        var subscriptionId = CurrentSubscriptionId();

        var company = await _context.Companies
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException($"Company {id} was not found.");

        if (company.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("You do not have access to this company.");
        }

        return ToResponse(company);
    }

    public async Task<IEnumerable<CompanyResponseDto>> GetAllAsync()
    {
        var subscriptionId = CurrentSubscriptionId();

        var companies = await _context.Companies
            .AsNoTracking()
            .Where(c => c.SubscriptionId == subscriptionId)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return companies.Select(ToResponse).ToList();
    }

    public async Task<CompanyResponseDto> UpdateAsync(int id, UpdateCompanyDto dto)
    {
        var subscriptionId = CurrentSubscriptionId();

        var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException($"Company {id} was not found.");

        if (company.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("You do not have access to this company.");
        }

        company.Name = dto.Name;
        company.Address = dto.Address;
        company.Phone = dto.Phone;
        company.Email = dto.Email;
        company.Website = dto.Website;
        company.IsActive = dto.IsActive;
        company.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ToResponse(company);
    }

    public async Task DeleteAsync(int id)
    {
        var subscriptionId = CurrentSubscriptionId();

        var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException($"Company {id} was not found.");

        if (company.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("You do not have access to this company.");
        }

        var hasActiveBranches = await _context.Branches
            .AnyAsync(b => b.CompanyId == id && b.IsActive);

        if (hasActiveBranches)
        {
            throw new InvalidOperationException("Cannot delete a company with active branches. Remove all branches first.");
        }

        if (!string.IsNullOrWhiteSpace(company.LogoPath))
        {
            DeleteLogoFromDisk(company.LogoPath);
        }

        _context.Companies.Remove(company);
        await _context.SaveChangesAsync();
    }

    public async Task<CompanyResponseDto> UploadLogoAsync(int id, IFormFile logo)
    {
        var subscriptionId = CurrentSubscriptionId();

        if (logo is null || logo.Length == 0)
        {
            throw new InvalidOperationException("Logo file is required.");
        }

        if (logo.Length > MaxLogoBytes)
        {
            throw new InvalidOperationException("Logo file size must not exceed 2 MB.");
        }

        if (!AllowedImageTypes.TryGetValue(logo.ContentType ?? string.Empty, out var permittedExtensions))
        {
            throw new InvalidOperationException("Logo must be a JPEG, PNG, or WebP image.");
        }

        var extension = Path.GetExtension(logo.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !permittedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Logo file extension does not match the declared image type.");
        }

        var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException($"Company {id} was not found.");

        if (company.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("You do not have access to this company.");
        }

        var webRootPath = ResolveWebRootPath();
        var absoluteDirectory = Path.Combine(webRootPath, LogoRelativeDirectory);
        Directory.CreateDirectory(absoluteDirectory);

        var fileName = $"company_{company.Id}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{extension}";
        var absoluteFilePath = Path.Combine(absoluteDirectory, fileName);

        if (!string.IsNullOrWhiteSpace(company.LogoPath))
        {
            DeleteLogoFromDisk(company.LogoPath);
        }

        await using (var fileStream = new FileStream(absoluteFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await logo.CopyToAsync(fileStream);
        }

        company.LogoPath = $"{LogoRelativeDirectory}/{fileName}";
        company.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ToResponse(company);
    }

    private CompanyResponseDto ToResponse(Company company)
    {
        var dto = _mapper.Map<CompanyResponseDto>(company);
        dto.LogoUrl = BuildLogoUrl(company.LogoPath);
        return dto;
    }

    private string? BuildLogoUrl(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return null;
        }

        var request = _httpContextAccessor.HttpContext?.Request;
        if (request is null)
        {
            return relativePath;
        }

        var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
        return $"{baseUrl.TrimEnd('/')}/{relativePath.TrimStart('/')}";
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

    private void DeleteLogoFromDisk(string relativePath)
    {
        var webRootPath = ResolveWebRootPath();
        var absolute = Path.Combine(webRootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(absolute))
        {
            File.Delete(absolute);
        }
    }

    private int CurrentSubscriptionId()
    {
        return _httpContextAccessor.HttpContext?.User.GetSubscriptionId()
            ?? throw new UnauthorizedAccessException("No authenticated user on the request.");
    }
}
