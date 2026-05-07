import { Routes } from '@angular/router';

const GRATUITY_ROUTES: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'rules' },

  {
    path: 'rules',
    loadComponent: () =>
      import('./gratuity-rule/rule-list/rule-list.component').then((m) => m.RuleListComponent),
  },
  {
    path: 'calculations',
    loadComponent: () =>
      import('./gratuity-calculation/calculation-list/calculation-list.component').then(
        (m) => m.CalculationListComponent,
      ),
  },
];

export default GRATUITY_ROUTES;
