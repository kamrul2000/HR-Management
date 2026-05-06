import { Routes } from '@angular/router';

const ATTENDANCE_ROUTES: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'duty-slots' },
  {
    path: 'duty-slots',
    loadComponent: () =>
      import('./duty-slots/duty-slot-list/duty-slot-list.component').then(
        (m) => m.DutySlotListComponent,
      ),
  },
  {
    path: 'records',
    loadComponent: () =>
      import('./attendance/attendance-list/attendance-list.component').then(
        (m) => m.AttendanceListComponent,
      ),
  },
  {
    path: 'entry',
    loadComponent: () =>
      import('./attendance/attendance-entry/attendance-entry.component').then(
        (m) => m.AttendanceEntryComponent,
      ),
  },
  {
    path: 'summary',
    loadComponent: () =>
      import('./attendance/attendance-summary/attendance-summary.component').then(
        (m) => m.AttendanceSummaryComponent,
      ),
  },
  {
    path: 'off-days',
    loadComponent: () =>
      import('./off-days/off-days-config/off-days-config.component').then(
        (m) => m.OffDaysConfigComponent,
      ),
  },
  {
    path: 'holiday-calendar',
    loadComponent: () =>
      import('./holiday-calendar/holiday-list/holiday-list.component').then(
        (m) => m.HolidayListComponent,
      ),
  },
];

export default ATTENDANCE_ROUTES;
