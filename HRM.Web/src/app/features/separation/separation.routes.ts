import { Routes } from '@angular/router';

const SEPARATION_ROUTES: Routes = [
  {
    path: '',
    pathMatch: 'full',
    loadComponent: () =>
      import('./employee-separation/separation-list/separation-list.component').then(
        (m) => m.SeparationListComponent,
      ),
  },
  {
    path: 'reasons',
    loadComponent: () =>
      import('./separation-reason/reason-list/reason-list.component').then(
        (m) => m.ReasonListComponent,
      ),
  },
  {
    path: ':id',
    loadComponent: () =>
      import('./employee-separation/separation-detail/separation-detail.component').then(
        (m) => m.SeparationDetailComponent,
      ),
  },
];

export default SEPARATION_ROUTES;
