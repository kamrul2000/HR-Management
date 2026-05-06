export interface OffDayResponse {
  id: number;
  dayOfWeek: number;
  dayName: string;
  branchId?: number | null;
  branchName?: string | null;
  isActive: boolean;
  subscriptionId: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateOffDayDto {
  dayOfWeek: number;
  branchId?: number | null;
}

export interface BulkSetOffDaysDto {
  branchId?: number | null;
  daysOfWeek: number[];
}

export const DAY_NAMES = [
  'Sunday',
  'Monday',
  'Tuesday',
  'Wednesday',
  'Thursday',
  'Friday',
  'Saturday',
];
