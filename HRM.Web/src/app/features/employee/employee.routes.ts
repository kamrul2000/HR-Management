import { Routes } from '@angular/router';

const EMPLOYEE_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./employee-list/employee-list.component').then((m) => m.EmployeeListComponent),
  },
  {
    path: 'new',
    loadComponent: () =>
      import('./employee-form/employee-form.component').then((m) => m.EmployeeFormComponent),
  },
  // Sidebar deep-link from "Additional Info" — pictograms direct user to the
  // employee list; from there they pick someone and explore the tabs.
  { path: 'additional-info', redirectTo: '', pathMatch: 'full' },
  {
    path: ':id',
    loadComponent: () =>
      import('./employee-detail/employee-detail.component').then((m) => m.EmployeeDetailComponent),
  },
  {
    path: ':id/edit',
    loadComponent: () =>
      import('./employee-form/employee-form.component').then((m) => m.EmployeeFormComponent),
  },
];

export default EMPLOYEE_ROUTES;
