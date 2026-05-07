import { Routes } from '@angular/router';

import { authGuard, guestGuard } from './core/auth/auth.guard';
import { ShellComponent } from './layout/shell/shell.component';

export const routes: Routes = [
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./features/auth/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: '',
    component: ShellComponent,
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./features/dashboard/dashboard.component').then((m) => m.DashboardComponent),
      },
      {
        path: 'organization',
        loadChildren: () => import('./features/organization/organization.routes'),
      },
      {
        path: 'employees',
        loadChildren: () => import('./features/employee/employee.routes'),
      },
      {
        path: 'attendance',
        loadChildren: () => import('./features/attendance/attendance.routes'),
      },
      {
        path: 'leave',
        loadChildren: () => import('./features/leave/leave.routes'),
      },
      {
        path: 'overtime',
        loadChildren: () => import('./features/overtime/overtime.routes'),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
