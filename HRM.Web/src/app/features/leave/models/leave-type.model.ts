export type GenderRestriction = 'All' | 'Male' | 'Female';

export interface LeaveTypeResponse {
  id: number;
  name: string;
  code: string;
  description?: string | null;
  isPaid: boolean;
  isCarryForward: boolean;
  maxCarryForwardDays: number;
  requiresApproval: boolean;
  requiresDocument: boolean;
  minNoticeDays: number;
  maxConsecutiveDays?: number | null;
  genderRestriction?: GenderRestriction | string | null;
  isActive: boolean;
  subscriptionId: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateLeaveTypeDto {
  name: string;
  code: string;
  description?: string | null;
  isPaid: boolean;
  isCarryForward: boolean;
  maxCarryForwardDays: number;
  requiresApproval: boolean;
  requiresDocument: boolean;
  minNoticeDays: number;
  maxConsecutiveDays?: number | null;
  genderRestriction?: GenderRestriction | null;
}

export interface UpdateLeaveTypeDto extends CreateLeaveTypeDto {
  isActive: boolean;
}
