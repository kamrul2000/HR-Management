using HRM.Core.DTOs.SeparationReason;

namespace HRM.API.Services.Interfaces;

public interface ISeparationReasonService
{
    Task<SeparationReasonResponseDto> CreateAsync(CreateSeparationReasonDto dto);
    Task<SeparationReasonResponseDto> GetByIdAsync(int id);
    Task<IEnumerable<SeparationReasonResponseDto>> GetAllAsync();
    Task<IEnumerable<SeparationReasonResponseDto>> GetBySeparationTypeAsync(string separationType);
    Task<SeparationReasonResponseDto> UpdateAsync(int id, UpdateSeparationReasonDto dto);
    Task DeleteAsync(int id);
}
