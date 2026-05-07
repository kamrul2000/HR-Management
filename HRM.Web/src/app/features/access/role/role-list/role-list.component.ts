import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import {
  Component,
  OnInit,
  TemplateRef,
  ViewChild,
  computed,
  inject,
  signal,
} from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { heroPlus, heroPencilSquare, heroTrash, heroLockClosed } from '@ng-icons/heroicons/outline';

import { ConfirmService } from '../../../../core/services/confirm.service';
import { ToastService } from '../../../../core/services/toast.service';
import { DataTableComponent } from '../../../../shared/components/data-table/data-table.component';
import { TableColumn } from '../../../../shared/components/data-table/data-table.types';
import { DrawerComponent } from '../../../../shared/components/drawer/drawer.component';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';
import {
  CreateRoleDto,
  RoleResponse,
  UpdateRoleDto,
} from '../../models/role.model';
import { RoleService } from '../../services/role.service';

@Component({
  selector: 'hrm-role-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterLink,
    NgIcon,
    PageHeaderComponent,
    DataTableComponent,
    StatusBadgeComponent,
    DrawerComponent,
  ],
  providers: [provideIcons({ heroPlus, heroPencilSquare, heroTrash, heroLockClosed })],
  templateUrl: './role-list.component.html',
})
export class RoleListComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(RoleService);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmService);

  readonly rows = signal<RoleResponse[]>([]);
  readonly loading = signal(true);
  readonly drawerOpen = signal(false);
  readonly editing = signal<RoleResponse | null>(null);
  readonly saving = signal(false);

  readonly form = this.fb.nonNullable.group({
    roleName: ['', [Validators.required, Validators.maxLength(100)]],
    description: [''],
    isActive: [true],
  });

  readonly isEdit = computed(() => !!this.editing());

  @ViewChild('countsTpl',     { static: true }) countsTpl!:    TemplateRef<{ $implicit: RoleResponse }>;
  @ViewChild('activeTpl',     { static: true }) activeTpl!:    TemplateRef<{ $implicit: RoleResponse }>;
  @ViewChild('actionsTpl',    { static: true }) actionsTpl!:   TemplateRef<{ $implicit: RoleResponse }>;

  columns: TableColumn<RoleResponse>[] = [];

  ngOnInit(): void {
    this.columns = [
      { key: 'roleName',    label: 'Role Name' },
      { key: 'description', label: 'Description' },
      { key: 'counts',      label: 'Users · Permissions', template: this.countsTpl, align: 'center', width: '180px' },
      { key: 'isActive',    label: 'Status', template: this.activeTpl, align: 'center', width: '110px' },
    ];
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.service.getAll().subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) this.rows.set(res.data);
      },
      error: () => this.loading.set(false),
    });
  }

  openCreate(): void {
    this.editing.set(null);
    this.form.reset({ roleName: '', description: '', isActive: true });
    this.drawerOpen.set(true);
  }

  openEdit(row: RoleResponse): void {
    this.editing.set(row);
    this.form.patchValue({
      roleName: row.roleName,
      description: row.description ?? '',
      isActive: row.isActive,
    });
    this.drawerOpen.set(true);
  }

  closeDrawer(): void {
    this.drawerOpen.set(false);
    this.editing.set(null);
  }

  submit(): void {
    if (this.saving()) return;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const raw = this.form.getRawValue();
    this.saving.set(true);

    if (this.editing()) {
      const dto: UpdateRoleDto = {
        roleName: raw.roleName.trim(),
        description: raw.description?.trim() || null,
        isActive: raw.isActive,
      };
      this.service.update(this.editing()!.id, dto).subscribe({
        next: (res) => this.afterSave(res, 'updated'),
        error: (err: HttpErrorResponse) => this.afterError(err),
      });
    } else {
      const dto: CreateRoleDto = {
        roleName: raw.roleName.trim(),
        description: raw.description?.trim() || null,
      };
      this.service.create(dto).subscribe({
        next: (res) => this.afterSave(res, 'created'),
        error: (err: HttpErrorResponse) => this.afterError(err),
      });
    }
  }

  delete(row: RoleResponse): void {
    this.confirm.confirm({
      title: 'Delete role',
      message: `Delete role "${row.roleName}"? This will revoke all assignments and permissions.`,
      confirmLabel: 'Delete',
      danger: true,
    }).subscribe((ok) => {
      if (!ok) return;
      this.service.delete(row.id).subscribe({
        next: (res) => {
          if (res.success) {
            this.toast.success('Role deleted.');
            this.rows.set(this.rows().filter((r) => r.id !== row.id));
          } else {
            this.toast.error(res.message || 'Delete failed.');
          }
        },
        error: (err) => this.toast.error(err.error?.message || 'Delete failed.'),
      });
    });
  }

  private afterSave(res: { success: boolean; message: string }, what: string): void {
    this.saving.set(false);
    if (res.success) {
      this.toast.success(`Role ${what}.`);
      this.closeDrawer();
      this.load();
    } else {
      this.toast.error(res.message || 'Save failed.');
    }
  }

  private afterError(err: HttpErrorResponse): void {
    this.saving.set(false);
    this.toast.error(err.error?.message || 'Save failed.');
  }
}
