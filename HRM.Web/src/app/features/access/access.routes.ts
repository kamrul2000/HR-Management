import { Routes } from '@angular/router';

const ACCESS_ROUTES: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'roles' },

  {
    path: 'roles',
    loadComponent: () =>
      import('./role/role-list/role-list.component').then((m) => m.RoleListComponent),
  },
  {
    path: 'user-roles',
    loadComponent: () =>
      import('./user-role/user-role-list/user-role-list.component').then(
        (m) => m.UserRoleListComponent,
      ),
  },
  {
    path: 'permissions',
    loadComponent: () =>
      import('./permission/permission-matrix/permission-matrix.component').then(
        (m) => m.PermissionMatrixComponent,
      ),
  },
];

export default ACCESS_ROUTES;
