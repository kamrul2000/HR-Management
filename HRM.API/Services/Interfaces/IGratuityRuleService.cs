using HRM.Core.DTOs.GratuitySetup;

namespace HRM.API.Services.Interfaces;

public interface IGratuityRuleService
{
    Task<GratuityRuleResponseDto> CreateAsync(CreateGratuityRuleDto dto);
    Task<GratuityRuleResponseDto> GetByIdAsync(int id);
    Task<GratuityRuleResponseDto> GetActiveAsync();
    Task<IEnumerable<GratuityRuleResponseDto>> GetAllAsync();
    Task<GratuityRuleResponseDto> UpdateAsync(int id, UpdateGratuityRuleDto dto);
    Task<GratuityPreviewResultDto> PreviewAsync(GratuityPreviewDto dto);
    Task DeleteAsync(int id);
}
