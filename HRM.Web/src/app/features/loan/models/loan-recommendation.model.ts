export type RecommendationDecision = 'Recommend' | 'Reject';

export interface LoanRecommendationResponse {
  id: number;
  loanApplicationId: number;
  applicationNo: string;
  employeeId: number;
  employeeCode: string;
  employeeFullName: string;
  requestedAmount: number;
  requestedAmountFormatted?: string;
  requestedTenureMonths: number;
  recommendedById: number;
  decision: RecommendationDecision | string;
  decisionLabel?: string;
  recommendedAmount?: number | null;
  recommendedAmountFormatted?: string | null;
  recommendedTenureMonths?: number | null;
  amountDifference?: number | null;
  remarks: string;
  subscriptionId: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateRecommendationDto {
  loanApplicationId: number;
  decision: RecommendationDecision;
  recommendedAmount?: number | null;
  recommendedTenureMonths?: number | null;
  remarks: string;
}
