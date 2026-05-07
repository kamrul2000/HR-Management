export interface PermissionResponse {
  id: number;
  roleId: number;
  roleName: string;
  moduleCode: string;
  moduleLabel: string;
  canView: boolean;
  canCreate: boolean;
  canEdit: boolean;
  canDelete: boolean;
  canApprove: boolean;
  canExport: boolean;
  subscriptionId: number;
  createdAt: string;
  updatedAt: string;
}

export interface UpsertPermissionDto {
  roleId: number;
  moduleCode: string;
  canView: boolean;
  canCreate: boolean;
  canEdit: boolean;
  canDelete: boolean;
  canApprove: boolean;
  canExport: boolean;
}

export interface ModulePermissionDto {
  moduleCode: string;
  canView: boolean;
  canCreate: boolean;
  canEdit: boolean;
  canDelete: boolean;
  canApprove: boolean;
  canExport: boolean;
}

export interface BulkUpsertPermissionsDto {
  roleId: number;
  permissions: ModulePermissionDto[];
}

export interface UserPermissionSummary {
  userId: number;
  roles: string[];
  permissions: PermissionResponse[];
}

/** Module catalog used to render the permissions matrix. Mirrors the sidebar's `module` codes. */
export interface ModuleDef {
  code: string;
  label: string;
  group: string;
}

export const MODULE_CATALOG: ModuleDef[] = [
  { code: 'COMPANY',     label: 'Companies',          group: 'Organization' },
  { code: 'BRANCH',      label: 'Branches',           group: 'Organization' },
  { code: 'DEPARTMENT',  label: 'Departments',        group: 'Organization' },
  { code: 'DESIGNATION', label: 'Designations',       group: 'Organization' },
  { code: 'EMPLOYEE',    label: 'Employees',          group: 'People' },
  { code: 'ATTENDANCE',  label: 'Attendance',         group: 'Time' },
  { code: 'LEAVE',       label: 'Leave',              group: 'Time' },
  { code: 'OVERTIME',    label: 'Overtime',           group: 'Time' },
  { code: 'SALARY',      label: 'Salary',             group: 'Payroll' },
  { code: 'BONUS',       label: 'Bonus',              group: 'Payroll' },
  { code: 'LOAN',        label: 'Loans',              group: 'Payroll' },
  { code: 'TAX',         label: 'Tax',                group: 'Payroll' },
  { code: 'PF',          label: 'Provident Fund',     group: 'Payroll' },
  { code: 'GRATUITY',    label: 'Gratuity',           group: 'Separation' },
  { code: 'SEPARATION',  label: 'Separation',         group: 'Separation' },
  { code: 'ROLE',        label: 'Access Control',     group: 'System' },
];
