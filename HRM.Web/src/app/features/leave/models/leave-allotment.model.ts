import { BulkCreateResult } from '../../../core/models/api-response.model';

export interface LeaveAllotmentResponse {
  id: number;
  employeeId: number;
  employeeFullName?: string;
  employeeCode?: string;
  leaveTypeId: number;
  leaveTypeName?: string;
  leaveTypeCode?: string;
  year: number;
  allocatedDays: number;
  usedDays: number;
  carriedForwardDays: number;
  remainingDays: number;
  isActive: boolean;
  subscriptionId: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateLeaveAllotmentDto {
  employeeId: number;
  leaveTypeId: number;
  year: number;
  allocatedDays: number;
  carriedForwardDays?: number;
}

export interface UpdateLeaveAllotmentDto {
  allocatedDays: number;
  carriedForwardDays?: number;
  isActive: boolean;
}

export interface LeaveAllotmentFilter {
  employeeId?: number;
  leaveTypeId?: number;
  year?: number;
  branchId?: number;
  pageNumber?: number;
  pageSize?: number;
}

export interface BulkAllotmentDto {
  year: number;
  leaveTypeId: number;
  allocatedDays: number;
  employeeIds?: number[];
  branchId?: number;
}

export type BulkAllotmentResult = BulkCreateResult;

export interface LeaveBalanceDto {
  employeeId: number;
  leaveTypeId: number;
  year: number;
  allocatedDays: number;
  usedDays: number;
  carriedForwardDays: number;
  remainingDays: number;
}
