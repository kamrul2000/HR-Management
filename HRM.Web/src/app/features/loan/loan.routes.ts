import { Routes } from '@angular/router';

const LOAN_ROUTES: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'applications' },

  {
    path: 'applications',
    loadComponent: () =>
      import('./loan-application/application-list/application-list.component').then(
        (m) => m.ApplicationListComponent,
      ),
  },
  {
    path: 'applications/:id',
    loadComponent: () =>
      import('./loan-application/application-detail/application-detail.component').then(
        (m) => m.ApplicationDetailComponent,
      ),
  },

  {
    path: 'approvals',
    loadComponent: () =>
      import('./loan-approval/approval-list/approval-list.component').then(
        (m) => m.ApprovalListComponent,
      ),
  },

  {
    path: 'active',
    loadComponent: () =>
      import('./employee-loan/active-list/active-list.component').then(
        (m) => m.ActiveListComponent,
      ),
  },
  {
    path: 'active/:id',
    loadComponent: () =>
      import('./employee-loan/active-detail/active-detail.component').then(
        (m) => m.ActiveDetailComponent,
      ),
  },

  {
    path: 'installments',
    loadComponent: () =>
      import('./loan-installment/installment-list/installment-list.component').then(
        (m) => m.InstallmentListComponent,
      ),
  },
];

export default LOAN_ROUTES;
