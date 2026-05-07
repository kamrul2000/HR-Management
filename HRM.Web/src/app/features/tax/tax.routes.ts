import { Routes } from '@angular/router';

const TAX_ROUTES: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'slabs' },

  {
    path: 'slabs',
    loadComponent: () =>
      import('./tax-slab/slab-list/slab-list.component').then((m) => m.SlabListComponent),
  },
  {
    path: 'slabs/new',
    loadComponent: () =>
      import('./tax-slab/slab-form/slab-form.component').then((m) => m.SlabFormComponent),
  },
  {
    path: 'slabs/:id',
    loadComponent: () =>
      import('./tax-slab/slab-detail/slab-detail.component').then((m) => m.SlabDetailComponent),
  },
  {
    path: 'slabs/:id/edit',
    loadComponent: () =>
      import('./tax-slab/slab-form/slab-form.component').then((m) => m.SlabFormComponent),
  },

  {
    path: 'exclusions',
    loadComponent: () =>
      import('./tax-exclusion/exclusion-list/exclusion-list.component').then(
        (m) => m.ExclusionListComponent,
      ),
  },
];

export default TAX_ROUTES;
