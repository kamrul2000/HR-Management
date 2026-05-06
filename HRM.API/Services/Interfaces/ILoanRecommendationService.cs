using HRM.Core.DTOs.LoanRecommendation;

namespace HRM.API.Services.Interfaces;

public interface ILoanRecommendationService
{
    Task<LoanRecommendationResponseDto> RecommendAsync(CreateRecommendationDto dto);
    Task<LoanRecommendationResponseDto> GetByIdAsync(int id);
    Task<LoanRecommendationResponseDto> GetByLoanApplicationAsync(int loanApplicationId);
    Task<IEnumerable<LoanRecommendationResponseDto>> GetAllAsync();
}
