using HRM.Core.DTOs.GratuityCalculation;

namespace HRM.API.Services.Interfaces;

public interface IGratuityCalculationService
{
    Task<GratuityCalculationResponseDto> ComputeAsync(ComputeGratuityDto dto);
    Task<GratuityCalculationResponseDto> GetByIdAsync(int id);
    Task<GratuityCalculationResponseDto> GetByEmployeeAsync(int employeeId);
    Task<IEnumerable<GratuityCalculationResponseDto>> GetAllAsync();
    Task<GratuityReportDto> GetReportAsync(int? branchId = null, string? status = null);
    Task<GratuityCalculationResponseDto> FinalizeAsync(int id);
    Task<GratuityCalculationResponseDto> CancelAsync(int id, string reason);
}
