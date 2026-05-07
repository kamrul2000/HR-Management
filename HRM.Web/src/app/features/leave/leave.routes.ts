import { Routes } from '@angular/router';

const LEAVE_ROUTES: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'types' },
  {
    path: 'types',
    loadComponent: () =>
      import('./leave-type/leave-type-list/leave-type-list.component').then(
        (m) => m.LeaveTypeListComponent,
      ),
  },
  {
    path: 'allotments',
    loadComponent: () =>
      import('./leave-allotment/allotment-list/allotment-list.component').then(
        (m) => m.AllotmentListComponent,
      ),
  },
  {
    path: 'allotments/bulk',
    loadComponent: () =>
      import('./leave-allotment/bulk-allotment/bulk-allotment.component').then(
        (m) => m.BulkAllotmentComponent,
      ),
  },
  {
    path: 'applications',
    loadComponent: () =>
      import('./leave-application/application-list/application-list.component').then(
        (m) => m.ApplicationListComponent,
      ),
  },
  {
    path: 'applications/new',
    loadComponent: () =>
      import('./leave-application/application-form/application-form.component').then(
        (m) => m.ApplicationFormComponent,
      ),
  },
];

export default LEAVE_ROUTES;
