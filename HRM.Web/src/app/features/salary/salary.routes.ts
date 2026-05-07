import { Routes } from '@angular/router';

const SALARY_ROUTES: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'heads' },

  {
    path: 'heads',
    loadComponent: () =>
      import('./salary-heads/salary-head-list/salary-head-list.component').then(
        (m) => m.SalaryHeadListComponent,
      ),
  },

  {
    path: 'structures',
    loadComponent: () =>
      import('./salary-structure/structure-list/structure-list.component').then(
        (m) => m.StructureListComponent,
      ),
  },
  {
    path: 'structures/new',
    loadComponent: () =>
      import('./salary-structure/structure-form/structure-form.component').then(
        (m) => m.StructureFormComponent,
      ),
  },
  {
    path: 'structures/:id',
    loadComponent: () =>
      import('./salary-structure/structure-detail/structure-detail.component').then(
        (m) => m.StructureDetailComponent,
      ),
  },
  {
    path: 'structures/:id/edit',
    loadComponent: () =>
      import('./salary-structure/structure-form/structure-form.component').then(
        (m) => m.StructureFormComponent,
      ),
  },

  {
    path: 'calculations',
    loadComponent: () =>
      import('./salary-calculation/calculation-list/calculation-list.component').then(
        (m) => m.CalculationListComponent,
      ),
  },
  {
    path: 'calculations/run',
    loadComponent: () =>
      import('./salary-calculation/run-salary/run-salary.component').then(
        (m) => m.RunSalaryComponent,
      ),
  },
  {
    path: 'calculations/:id',
    loadComponent: () =>
      import('./salary-calculation/payslip/payslip.component').then((m) => m.PayslipComponent),
  },

  {
    path: 'bonus',
    loadComponent: () =>
      import('./bonus/bonus-list/bonus-list.component').then((m) => m.BonusListComponent),
  },
];

export default SALARY_ROUTES;
