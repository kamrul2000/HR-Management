export type HolidayType = 'Public' | 'Optional' | 'Organizational';

export interface HolidayResponse {
  id: number;
  holidayName: string;
  holidayDate: string;
  holidayDateFormatted?: string;
  holidayType: HolidayType;
  description?: string | null;
  isRecurringYearly: boolean;
  branchId?: number | null;
  branchName?: string | null;
  isActive: boolean;
  subscriptionId: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateHolidayDto {
  holidayName: string;
  holidayDate: string;
  holidayType: HolidayType;
  description?: string | null;
  isRecurringYearly: boolean;
  branchId?: number | null;
}

export interface UpdateHolidayDto extends CreateHolidayDto {
  isActive: boolean;
}

export interface HolidayFilter {
  year?: number;
  month?: number;
  branchId?: number;
  holidayType?: HolidayType | string;
  isActive?: boolean;
}

export const HOLIDAY_TYPES: HolidayType[] = ['Public', 'Optional', 'Organizational'];
