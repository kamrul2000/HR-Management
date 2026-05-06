/** All times are sent/received as `HH:mm:ss` strings (TimeSpan format). */
export interface DutySlotResponse {
  id: number;
  slotName: string;
  startTime: string;
  endTime: string;
  breakDurationMinutes: number;
  lateToleranceMinutes: number;
  totalWorkingHours: number;
  isNightShift: boolean;
  isActive: boolean;
  subscriptionId: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateDutySlotDto {
  slotName: string;
  startTime: string;
  endTime: string;
  breakDurationMinutes: number;
  lateToleranceMinutes: number;
}

export interface UpdateDutySlotDto extends CreateDutySlotDto {
  isActive: boolean;
}
