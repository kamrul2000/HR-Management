import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import {
  Component,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { heroCheck, heroArrowPath } from '@ng-icons/heroicons/outline';

import { ToastService } from '../../../../core/services/toast.service';
import { LoadingSkeletonComponent } from '../../../../shared/components/loading-skeleton/loading-skeleton.component';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import {
  BulkUpsertPermissionsDto,
  MODULE_CATALOG,
  ModuleDef,
  ModulePermissionDto,
  PermissionResponse,
} from '../../models/permission.model';
import { RoleResponse } from '../../models/role.model';
import { PermissionService } from '../../services/permission.service';
import { RoleService } from '../../services/role.service';

const ACTIONS = ['canView', 'canCreate', 'canEdit', 'canDelete', 'canApprove', 'canExport'] as const;
type Action = typeof ACTIONS[number];

interface MatrixRow extends ModuleDef {
  canView: boolean;
  canCreate: boolean;
  canEdit: boolean;
  canDelete: boolean;
  canApprove: boolean;
  canExport: boolean;
}

@Component({
  selector: 'hrm-permission-matrix',
  standalone: true,
  imports: [CommonModule, FormsModule, NgIcon, PageHeaderComponent, LoadingSkeletonComponent],
  providers: [provideIcons({ heroCheck, heroArrowPath })],
  templateUrl: './permission-matrix.component.html',
  styleUrl: './permission-matrix.component.scss',
})
export class PermissionMatrixComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly perms = inject(PermissionService);
  private readonly rolesSvc = inject(RoleService);
  private readonly toast = inject(ToastService);

  readonly roles = signal<RoleResponse[]>([]);
  readonly selectedRoleId = signal<number>(0);
  readonly matrix = signal<MatrixRow[]>([]);
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly dirty = signal(false);

  readonly actions = ACTIONS;
  readonly actionLabels: Record<Action, string> = {
    canView: 'View',
    canCreate: 'Create',
    canEdit: 'Edit',
    canDelete: 'Delete',
    canApprove: 'Approve',
    canExport: 'Export',
  };

  /** Group rows by category for visual grouping. */
  readonly grouped = computed(() => {
    const groups: Record<string, MatrixRow[]> = {};
    this.matrix().forEach((row) => {
      (groups[row.group] ||= []).push(row);
    });
    return Object.entries(groups).map(([group, rows]) => ({ group, rows }));
  });

  readonly selectedRole = computed(() => this.roles().find((r) => r.id === this.selectedRoleId()) ?? null);

  ngOnInit(): void {
    this.rolesSvc.getActive().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.roles.set(res.data);
          const queryRoleId = +(this.route.snapshot.queryParamMap.get('roleId') ?? 0);
          const initial = queryRoleId && res.data.some((r) => r.id === queryRoleId)
            ? queryRoleId
            : res.data[0]?.id ?? 0;
          if (initial) this.selectRole(initial);
          else this.loading.set(false);
        } else {
          this.loading.set(false);
        }
      },
      error: () => this.loading.set(false),
    });
  }

  selectRole(roleId: number): void {
    this.selectedRoleId.set(roleId);
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { roleId },
      queryParamsHandling: 'merge',
      replaceUrl: true,
    });
    this.loadMatrix(roleId);
  }

  private loadMatrix(roleId: number): void {
    this.loading.set(true);
    this.perms.getByRole(roleId).subscribe({
      next: (res) => {
        this.loading.set(false);
        const existing = (res.success && res.data) ? res.data : [];
        const existingMap = new Map<string, PermissionResponse>(existing.map((p) => [p.moduleCode, p]));
        const rows: MatrixRow[] = MODULE_CATALOG.map((m) => {
          const p = existingMap.get(m.code);
          return {
            ...m,
            canView:    p?.canView ?? false,
            canCreate:  p?.canCreate ?? false,
            canEdit:    p?.canEdit ?? false,
            canDelete:  p?.canDelete ?? false,
            canApprove: p?.canApprove ?? false,
            canExport:  p?.canExport ?? false,
          };
        });
        this.matrix.set(rows);
        this.dirty.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  toggle(row: MatrixRow, action: Action): void {
    row[action] = !row[action];
    // View must be true if any other action is true.
    if (action !== 'canView' && row[action] && !row.canView) {
      row.canView = true;
    }
    this.matrix.set([...this.matrix()]);
    this.dirty.set(true);
  }

  toggleAllForRow(row: MatrixRow, value: boolean): void {
    ACTIONS.forEach((a) => (row[a] = value));
    this.matrix.set([...this.matrix()]);
    this.dirty.set(true);
  }

  toggleAllForAction(action: Action, value: boolean): void {
    this.matrix().forEach((r) => {
      r[action] = value;
      if (action !== 'canView' && value && !r.canView) r.canView = true;
    });
    this.matrix.set([...this.matrix()]);
    this.dirty.set(true);
  }

  reset(): void {
    if (this.selectedRoleId()) this.loadMatrix(this.selectedRoleId());
  }

  save(): void {
    if (this.saving() || !this.selectedRoleId()) return;
    const permissions: ModulePermissionDto[] = this.matrix()
      .filter((r) => ACTIONS.some((a) => r[a]))
      .map((r) => ({
        moduleCode: r.code,
        canView: r.canView,
        canCreate: r.canCreate,
        canEdit: r.canEdit,
        canDelete: r.canDelete,
        canApprove: r.canApprove,
        canExport: r.canExport,
      }));

    const dto: BulkUpsertPermissionsDto = {
      roleId: this.selectedRoleId(),
      permissions,
    };

    if (permissions.length === 0) {
      this.toast.error('Grant at least one permission before saving.');
      return;
    }

    this.saving.set(true);
    this.perms.bulkUpsert(dto).subscribe({
      next: (res) => {
        this.saving.set(false);
        if (res.success) {
          this.toast.success('Permissions saved.');
          this.dirty.set(false);
        } else {
          this.toast.error(res.message || 'Save failed.');
        }
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.toast.error(err.error?.message || 'Save failed.');
      },
    });
  }
}
