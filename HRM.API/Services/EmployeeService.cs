using AutoMapper;
using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.Employee;
using HRM.Core.Entities;
using HRM.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Services;

public class EmployeeService : IEmployeeService
{
    private const long MaxPhotoBytes = 1L * 1024 * 1024;
    private const string PhotoRelativeDirectory = "uploads/employee-photos";
    private const int MaxPageSize = 100;

    private static readonly IReadOnlyDictionary<string, string[]> AllowedImageTypes =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["image/jpeg"] = new[] { ".jpg", ".jpeg" },
            ["image/png"] = new[] { ".png" },
            ["image/webp"] = new[] { ".webp" }
        };

    private readonly IRepository<Employee> _employeeRepository;
    private readonly IRepository<Branch> _branchRepository;
    private readonly IRepository<Department> _departmentRepository;
    private readonly IRepository<Designation> _designationRepository;
    private readonly IRepository<Attendance> _attendanceRepository;
    private readonly IRepository<LeaveApplication> _leaveApplicationRepository;
    private readonly IRepository<SalaryCreate> _salaryCreateRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWebHostEnvironment _env;
    private readonly IMapper _mapper;

    public EmployeeService(
        IRepository<Employee> employeeRepository,
        IRepository<Branch> branchRepository,
        IRepository<Department> departmentRepository,
        IRepository<Designation> designationRepository,
        IRepository<Attendance> attendanceRepository,
        IRepository<LeaveApplication> leaveApplicationRepository,
        IRepository<SalaryCreate> salaryCreateRepository,
        IHttpContextAccessor httpContextAccessor,
        IWebHostEnvironment env,
        IMapper mapper)
    {
        _employeeRepository = employeeRepository;
        _branchRepository = branchRepository;
        _departmentRepository = departmentRepository;
        _designationRepository = designationRepository;
        _attendanceRepository = attendanceRepository;
        _leaveApplicationRepository = leaveApplicationRepository;
        _salaryCreateRepository = salaryCreateRepository;
        _httpContextAccessor = httpContextAccessor;
        _env = env;
        _mapper = mapper;
    }

    public async Task<EmployeeResponseDto> CreateAsync(CreateEmployeeDto dto)
    {
        var subscriptionId = GetSubscriptionId();
        await ValidateOrganizationalHierarchyAsync(dto.BranchId, dto.DepartmentId, dto.DesignationId, subscriptionId);

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        var emailExists = await _employeeRepository.Query()
            .AnyAsync(e => e.SubscriptionId == subscriptionId && e.Email.ToLower() == normalizedEmail);

        if (emailExists)
        {
            throw new InvalidOperationException("An employee with this email already exists.");
        }

        var employeeCode = await GenerateEmployeeCodeAsync(subscriptionId);
        var now = DateTime.UtcNow;

        var employee = _mapper.Map<Employee>(dto);
        employee.Email = normalizedEmail;
        employee.EmployeeCode = employeeCode;
        employee.FullName = $"{dto.FirstName.Trim()} {dto.LastName.Trim()}";
        employee.Status = "Active";
        employee.SubscriptionId = subscriptionId;
        employee.IsActive = true;
        employee.PhotoPath = null;
        employee.CreatedAt = now;
        employee.UpdatedAt = now;

        await _employeeRepository.AddAsync(employee);

        return await LoadResponseAsync(employee.Id, subscriptionId);
    }

    public async Task<EmployeeResponseDto> GetByIdAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();
        return await LoadResponseAsync(id, subscriptionId);
    }

    public async Task<PagedResultDto<EmployeeListDto>> GetFilteredAsync(EmployeeFilterDto filter)
    {
        var subscriptionId = GetSubscriptionId();

        var pageSize = Math.Clamp(filter.PageSize, 1, MaxPageSize);
        var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;

        var query = BaseQuery(subscriptionId).AsNoTracking();

        if (filter.BranchId is int branchId)
        {
            query = query.Where(e => e.BranchId == branchId);
        }

        if (filter.DepartmentId is int departmentId)
        {
            query = query.Where(e => e.DepartmentId == departmentId);
        }

        if (filter.DesignationId is int designationId)
        {
            query = query.Where(e => e.DesignationId == designationId);
        }

        if (!string.IsNullOrWhiteSpace(filter.EmploymentType))
        {
            var employmentType = filter.EmploymentType.Trim();
            query = query.Where(e => e.EmploymentType == employmentType);
        }

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            var status = filter.Status.Trim();
            query = query.Where(e => e.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(e =>
                EF.Functions.Like(e.FullName, $"%{term}%") ||
                EF.Functions.Like(e.Email, $"%{term}%") ||
                EF.Functions.Like(e.EmployeeCode, $"%{term}%"));
        }

        var totalCount = await query.CountAsync();

        var employees = await query
            .OrderBy(e => e.FullName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = employees.Select(MapToListDto).ToList();

        return new PagedResultDto<EmployeeListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<EmployeeListDto>> GetByBranchAsync(int branchId)
    {
        var subscriptionId = GetSubscriptionId();
        await EnsureBranchOwnershipAsync(branchId, subscriptionId);

        var employees = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .Where(e => e.BranchId == branchId)
            .OrderBy(e => e.FullName)
            .ToListAsync();

        return employees.Select(MapToListDto).ToList();
    }

    public async Task<IEnumerable<EmployeeListDto>> GetByDepartmentAsync(int departmentId)
    {
        var subscriptionId = GetSubscriptionId();
        await EnsureDepartmentOwnershipAsync(departmentId, subscriptionId);

        var employees = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .Where(e => e.DepartmentId == departmentId)
            .OrderBy(e => e.FullName)
            .ToListAsync();

        return employees.Select(MapToListDto).ToList();
    }

    public async Task<EmployeeResponseDto> UpdateAsync(int id, UpdateEmployeeDto dto)
    {
        var subscriptionId = GetSubscriptionId();

        var employee = await _employeeRepository.Query()
            .FirstOrDefaultAsync(e => e.Id == id)
            ?? throw new KeyNotFoundException($"Employee with ID {id} not found.");

        if (employee.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this employee.");
        }

        var hierarchyChanged =
            dto.BranchId != employee.BranchId ||
            dto.DepartmentId != employee.DepartmentId ||
            dto.DesignationId != employee.DesignationId;

        if (hierarchyChanged)
        {
            await ValidateOrganizationalHierarchyAsync(dto.BranchId, dto.DepartmentId, dto.DesignationId, subscriptionId);
        }

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        if (!string.Equals(normalizedEmail, employee.Email, StringComparison.OrdinalIgnoreCase))
        {
            var emailTaken = await _employeeRepository.Query()
                .AnyAsync(e =>
                    e.Id != id &&
                    e.SubscriptionId == subscriptionId &&
                    e.Email.ToLower() == normalizedEmail);

            if (emailTaken)
            {
                throw new InvalidOperationException("An employee with this email already exists.");
            }
        }

        employee.FirstName = dto.FirstName;
        employee.LastName = dto.LastName;
        employee.FullName = $"{dto.FirstName.Trim()} {dto.LastName.Trim()}";
        employee.Email = normalizedEmail;
        employee.Phone = dto.Phone;
        employee.DateOfBirth = dto.DateOfBirth;
        employee.Gender = dto.Gender;
        employee.MaritalStatus = dto.MaritalStatus;
        employee.NationalId = dto.NationalId;
        employee.JoiningDate = dto.JoiningDate;
        employee.ConfirmationDate = dto.ConfirmationDate;
        employee.Address = dto.Address;
        employee.BranchId = dto.BranchId;
        employee.DepartmentId = dto.DepartmentId;
        employee.DesignationId = dto.DesignationId;
        employee.EmploymentType = dto.EmploymentType;
        employee.Status = dto.Status;
        employee.IsActive = dto.IsActive;
        employee.UpdatedAt = DateTime.UtcNow;

        await _employeeRepository.UpdateAsync(employee);

        return await LoadResponseAsync(employee.Id, subscriptionId);
    }

    public async Task<EmployeeResponseDto> UploadPhotoAsync(int id, IFormFile photo)
    {
        var subscriptionId = GetSubscriptionId();

        if (photo is null || photo.Length == 0)
        {
            throw new InvalidOperationException("Photo file is required.");
        }

        if (photo.Length > MaxPhotoBytes)
        {
            throw new InvalidOperationException("Photo file size must not exceed 1 MB.");
        }

        if (!AllowedImageTypes.TryGetValue(photo.ContentType ?? string.Empty, out var permittedExtensions))
        {
            throw new InvalidOperationException("Photo must be a JPEG, PNG, or WebP image.");
        }

        var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !permittedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Photo file extension does not match the declared image type.");
        }

        var employee = await _employeeRepository.Query()
            .FirstOrDefaultAsync(e => e.Id == id)
            ?? throw new KeyNotFoundException($"Employee with ID {id} not found.");

        if (employee.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this employee.");
        }

        var webRootPath = ResolveWebRootPath();
        var absoluteDirectory = Path.Combine(webRootPath, PhotoRelativeDirectory);
        Directory.CreateDirectory(absoluteDirectory);

        var fileName = $"emp_{employee.Id}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{extension}";
        var absoluteFilePath = Path.Combine(absoluteDirectory, fileName);

        if (!string.IsNullOrWhiteSpace(employee.PhotoPath))
        {
            DeletePhotoFromDisk(employee.PhotoPath);
        }

        await using (var fileStream = new FileStream(absoluteFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await photo.CopyToAsync(fileStream);
        }

        employee.PhotoPath = $"{PhotoRelativeDirectory}/{fileName}";
        employee.UpdatedAt = DateTime.UtcNow;
        await _employeeRepository.UpdateAsync(employee);

        return await LoadResponseAsync(employee.Id, subscriptionId);
    }

    public async Task DeleteAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();

        var employee = await _employeeRepository.Query()
            .FirstOrDefaultAsync(e => e.Id == id)
            ?? throw new KeyNotFoundException($"Employee with ID {id} not found.");

        if (employee.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this employee.");
        }

        var hasAttendance = await _attendanceRepository.Query().AnyAsync(a => a.EmployeeId == id);
        var hasLeave = await _leaveApplicationRepository.Query().AnyAsync(l => l.EmployeeId == id);
        var hasSalary = await _salaryCreateRepository.Query().AnyAsync(s => s.EmployeeId == id);

        if (hasAttendance || hasLeave || hasSalary)
        {
            throw new InvalidOperationException("Cannot delete an employee with existing HR records. Deactivate the employee instead.");
        }

        if (!string.IsNullOrWhiteSpace(employee.PhotoPath))
        {
            DeletePhotoFromDisk(employee.PhotoPath);
        }

        await _employeeRepository.DeleteAsync(employee);
    }

    private IQueryable<Employee> BaseQuery(int subscriptionId)
    {
        return _employeeRepository
            .Query()
            .Include(e => e.Branch)
                .ThenInclude(b => b.Company)
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .Where(e => e.SubscriptionId == subscriptionId);
    }

    private async Task<EmployeeResponseDto> LoadResponseAsync(int employeeId, int subscriptionId)
    {
        var employee = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == employeeId);

        if (employee is null)
        {
            var existsForOtherTenant = await _employeeRepository.Query()
                .AnyAsync(e => e.Id == employeeId);

            if (existsForOtherTenant)
            {
                throw new UnauthorizedAccessException("Access denied to this employee.");
            }

            throw new KeyNotFoundException($"Employee with ID {employeeId} not found.");
        }

        var dto = _mapper.Map<EmployeeResponseDto>(employee);
        dto.PhotoUrl = BuildPhotoUrl(employee.PhotoPath);
        return dto;
    }

    private EmployeeListDto MapToListDto(Employee employee)
    {
        var dto = _mapper.Map<EmployeeListDto>(employee);
        dto.PhotoUrl = BuildPhotoUrl(employee.PhotoPath);
        return dto;
    }

    private async Task<string> GenerateEmployeeCodeAsync(int subscriptionId)
    {
        var count = await _employeeRepository.Query()
            .CountAsync(e => e.SubscriptionId == subscriptionId);
        return $"EMP-{(count + 1):D4}";
    }

    private async Task ValidateOrganizationalHierarchyAsync(
        int branchId, int departmentId, int designationId, int subscriptionId)
    {
        var branch = await _branchRepository.GetByIdAsync(branchId)
            ?? throw new KeyNotFoundException($"Branch with ID {branchId} not found.");
        if (branch.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this branch.");
        }
        if (!branch.IsActive)
        {
            throw new InvalidOperationException("Cannot assign an employee to an inactive branch.");
        }

        var department = await _departmentRepository.GetByIdAsync(departmentId)
            ?? throw new KeyNotFoundException($"Department with ID {departmentId} not found.");
        if (department.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this department.");
        }
        if (department.BranchId != branchId)
        {
            throw new InvalidOperationException($"Department {departmentId} does not belong to Branch {branchId}.");
        }
        if (!department.IsActive)
        {
            throw new InvalidOperationException("Cannot assign an employee to an inactive department.");
        }

        var designation = await _designationRepository.GetByIdAsync(designationId)
            ?? throw new KeyNotFoundException($"Designation with ID {designationId} not found.");
        if (designation.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this designation.");
        }
        if (designation.DepartmentId != departmentId)
        {
            throw new InvalidOperationException($"Designation {designationId} does not belong to Department {departmentId}.");
        }
        if (!designation.IsActive)
        {
            throw new InvalidOperationException("Cannot assign an employee to an inactive designation.");
        }
    }

    private async Task EnsureBranchOwnershipAsync(int branchId, int subscriptionId)
    {
        var branch = await _branchRepository.GetByIdAsync(branchId)
            ?? throw new KeyNotFoundException($"Branch with ID {branchId} not found.");

        if (branch.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this branch.");
        }
    }

    private async Task EnsureDepartmentOwnershipAsync(int departmentId, int subscriptionId)
    {
        var department = await _departmentRepository.GetByIdAsync(departmentId)
            ?? throw new KeyNotFoundException($"Department with ID {departmentId} not found.");

        if (department.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this department.");
        }
    }

    private string? BuildPhotoUrl(string? relativePath)
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

    private void DeletePhotoFromDisk(string relativePath)
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
