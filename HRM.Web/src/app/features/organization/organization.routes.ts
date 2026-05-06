import { Routes } from '@angular/router';

const ORGANIZATION_ROUTES: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'companies' },
  {
    path: 'companies',
    loadComponent: () =>
      import('./company/company-list/company-list.component').then((m) => m.CompanyListComponent),
  },
  {
    path: 'branches',
    loadComponent: () =>
      import('./branch/branch-list/branch-list.component').then((m) => m.BranchListComponent),
  },
  {
    path: 'departments',
    loadComponent: () =>
      import('./department/department-list/department-list.component').then(
        (m) => m.DepartmentListComponent,
      ),
  },
  {
    path: 'designations',
    loadComponent: () =>
      import('./designation/designation-list/designation-list.component').then(
        (m) => m.DesignationListComponent,
      ),
  },
];

export default ORGANIZATION_ROUTES;
