export type SeparationType = 'Resignation' | 'Termination' | 'Retirement' | 'Death' | 'Contract End' | string;

export interface SeparationReasonResponse {
  id: number;
  reasonName: string;
  separationType: SeparationType;
  separationTypeLabel?: string;
  description?: string | null;
  displayOrder: number;
  isActive: boolean;
  subscriptionId: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateSeparationReasonDto {
  reasonName: string;
  separationType: SeparationType;
  description?: string | null;
  displayOrder: number;
}

export interface UpdateSeparationReasonDto {
  reasonName: string;
  separationType: SeparationType;
  description?: string | null;
  displayOrder: number;
  isActive: boolean;
}
