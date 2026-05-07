import { Routes } from '@angular/router';

const OVERTIME_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./overtime-list/overtime-list.component').then((m) => m.OvertimeListComponent),
  },
  {
    path: 'new',
    loadComponent: () =>
      import('./overtime-form/overtime-form.component').then((m) => m.OvertimeFormComponent),
  },
  {
    path: 'summary',
    loadComponent: () =>
      import('./overtime-summary/overtime-summary.component').then((m) => m.OvertimeSummaryComponent),
  },
];

export default OVERTIME_ROUTES;
