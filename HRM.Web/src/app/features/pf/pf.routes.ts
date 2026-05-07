import { Routes } from '@angular/router';

const PF_ROUTES: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'contributions' },

  {
    path: 'contributions',
    loadComponent: () =>
      import('./pf-contribution/contribution-list/contribution-list.component').then(
        (m) => m.ContributionListComponent,
      ),
  },
  {
    path: 'interest',
    loadComponent: () =>
      import('./pf-interest/interest-list/interest-list.component').then(
        (m) => m.InterestListComponent,
      ),
  },
];

export default PF_ROUTES;
