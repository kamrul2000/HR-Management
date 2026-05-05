using HRM.Core.DTOs.Company;

namespace HRM.API.Services.Interfaces;

public interface ICompanyService
{
    Task<CompanyResponseDto> CreateAsync(CreateCompanyDto dto);
    Task<CompanyResponseDto> GetByIdAsync(int id);
    Task<IEnumerable<CompanyResponseDto>> GetAllAsync();
    Task<CompanyResponseDto> UpdateAsync(int id, UpdateCompanyDto dto);
    Task DeleteAsync(int id);
    Task<CompanyResponseDto> UploadLogoAsync(int id, IFormFile logo);
}
