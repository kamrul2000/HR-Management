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
import { NgIcon, provideIcons } from '@ng-icons/core';
import { heroPlus, heroXMark } from '@ng-icons/heroicons/outline';
import { forkJoin } from 'rxjs';

import { ConfirmService } from '../../../../core/services/confirm.service';
import { ToastService } from '../../../../core/services/toast.service';
import { AvatarComponent } from '../../../../shared/components/avatar/avatar.component';
import { DataTableComponent } from '../../../../shared/components/data-table/data-table.component';
import { TableColumn } from '../../../../shared/components/data-table/data-table.types';
import { DrawerComponent } from '../../../../shared/components/drawer/drawer.component';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';
import { RoleResponse } from '../../models/role.model';
import { AssignRoleDto, UserRoleResponse } from '../../models/user-role.model';
import { RoleService } from '../../services/role.service';
import { UserRoleService } from '../../services/user-role.service';

@Component({
  selector: 'hrm-user-role-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    NgIcon,
    PageHeaderComponent,
    DataTableComponent,
    StatusBadgeComponent,
    AvatarComponent,
    DrawerComponent,
  ],
  providers: [provideIcons({ heroPlus, heroXMark })],
  templateUrl: './user-role-list.component.html',
})
export class UserRoleListComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly userRoles = inject(UserRoleService);
  private readonly roles = inject(RoleService);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmService);

  readonly rows = signal<UserRoleResponse[]>([]);
  readonly availableRoles = signal<RoleResponse[]>([]);
  readonly loading = signal(true);
  readonly drawerOpen = signal(false);
  readonly saving = signal(false);

  readonly form = this.fb.nonNullable.group({
    userId: [0, [Validators.required, Validators.min(1)]],
    roleId: [0, [Validators.required, Validators.min(1)]],
  });

  @ViewChild('userCellTpl', { static: true }) userCellTpl!: TemplateRef<{ $implicit: UserRoleResponse }>;
  @ViewChild('roleCellTpl', { static: true }) roleCellTpl!: TemplateRef<{ $implicit: UserRoleResponse }>;
  @ViewChild('whenTpl',     { static: true }) whenTpl!:     TemplateRef<{ $implicit: UserRoleResponse }>;
  @ViewChild('actionsTpl',  { static: true }) actionsTpl!:  TemplateRef<{ $implicit: UserRoleResponse }>;

  columns: TableColumn<UserRoleResponse>[] = [];

  /** Distinct users derived from existing assignments — surfaced as a hint when assigning. */
  readonly knownUsers = computed(() => {
    const map = new Map<number, { id: number; name: string; email: string }>();
    this.rows().forEach((r) => {
      if (!map.has(r.userId)) {
        map.set(r.userId, { id: r.userId, name: r.userName, email: r.userEmail });
      }
    });
    return Array.from(map.values()).sort((a, b) => a.name.localeCompare(b.name));
  });

  ngOnInit(): void {
    this.columns = [
      { key: 'user',    label: 'User', template: this.userCellTpl },
      { key: 'role',    label: 'Role', template: this.roleCellTpl, width: '200px' },
      { key: 'when',    label: 'Assigned', template: this.whenTpl, width: '180px' },
    ];
    this.load();
  }

  load(): void {
    this.loading.set(true);
    forkJoin({
      assignments: this.userRoles.getAllActive(),
      roles: this.roles.getActive(),
    }).subscribe({
      next: ({ assignments, roles }) => {
        this.loading.set(false);
        if (assignments.success && assignments.data) this.rows.set(assignments.data);
        if (roles.success && roles.data) this.availableRoles.set(roles.data);
      },
      error: () => this.loading.set(false),
    });
  }

  openAssign(): void {
    this.form.reset({ userId: 0, roleId: 0 });
    this.drawerOpen.set(true);
  }

  closeDrawer(): void { this.drawerOpen.set(false); }

  pickKnownUser(userId: number): void {
    this.form.patchValue({ userId });
  }

  submit(): void {
    if (this.saving()) return;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const raw = this.form.getRawValue();
    const dto: AssignRoleDto = {
      userId: +raw.userId,
      roleId: +raw.roleId,
    };
    this.saving.set(true);
    this.userRoles.assign(dto).subscribe({
      next: (res) => {
        this.saving.set(false);
        if (res.success) {
          this.toast.success('Role assigned.');
          this.closeDrawer();
          this.load();
        } else {
          this.toast.error(res.message || 'Assignment failed.');
        }
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.toast.error(err.error?.message || 'Assignment failed.');
      },
    });
  }

  revoke(row: UserRoleResponse): void {
    this.confirm.confirm({
      title: 'Revoke role',
      message: `Revoke role "${row.roleName}" from ${row.userName}?`,
      confirmLabel: 'Revoke',
      danger: true,
    }).subscribe((ok) => {
      if (!ok) return;
      this.userRoles.revoke(row.id).subscribe({
        next: (res) => {
          if (res.success) {
            this.toast.success('Role revoked.');
            this.rows.set(this.rows().filter((r) => r.id !== row.id));
          } else {
            this.toast.error(res.message || 'Revoke failed.');
          }
        },
        error: (err) => this.toast.error(err.error?.message || 'Revoke failed.'),
      });
    });
  }
}
