import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { NgIcon, provideIcons } from '@ng-icons/core';
import {
  heroArrowLeft,
  heroCheckCircle,
  heroExclamationTriangle,
  heroUsers,
  heroBuildingOffice2,
} from '@ng-icons/heroicons/outline';

import { ToastService } from '../../../../core/services/toast.service';
import { AvatarComponent } from '../../../../shared/components/avatar/avatar.component';
import { LoadingSkeletonComponent } from '../../../../shared/components/loading-skeleton/loading-skeleton.component';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import { EmployeeResponse } from '../../../employee/models/employee.model';
import { EmployeeService } from '../../../employee/services/employee.service';
import { BranchResponse } from '../../../organization/models/branch.model';
import { BranchService } from '../../../organization/services/branch.service';
import { BulkAllotmentResult } from '../../models/leave-allotment.model';
import { LeaveTypeResponse } from '../../models/leave-type.model';
import { LeaveAllotmentService } from '../../services/leave-allotment.service';
import { LeaveTypeService } from '../../services/leave-type.service';

type Scope = 'branch' | 'manual';

@Component({
  selector: 'hrm-bulk-allotment',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    NgIcon,
    PageHeaderComponent,
    LoadingSkeletonComponent,
    AvatarComponent,
  ],
  providers: [
    provideIcons({
      heroArrowLeft,
      heroCheckCircle,
      heroExclamationTriangle,
      heroUsers,
      heroBuildingOffice2,
    }),
  ],
  templateUrl: './bulk-allotment.component.html',
  styleUrl: './bulk-allotment.component.scss',
})
export class BulkAllotmentComponent implements OnInit {
  private readonly service = inject(LeaveAllotmentService);
  private readonly leaveTypes = inject(LeaveTypeService);
  private readonly branches = inject(BranchService);
  private readonly employees = inject(EmployeeService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);

  // ── form state
  readonly year = signal<number>(new Date().getFullYear());
  readonly leaveTypeId = signal<number | null>(null);
  readonly allocatedDays = signal<number>(0);
  readonly scope = signal<Scope>('branch');
  readonly branchId = signal<number | null>(null);
  readonly selectedIds = signal<Set<number>>(new Set());

  // ── lookups
  readonly leaveTypeOptions = signal<LeaveTypeResponse[]>([]);
  readonly branchOptions = signal<BranchResponse[]>([]);
  readonly employees$ = signal<EmployeeResponse[]>([]);
  readonly loadingEmployees = signal(false);

  // ── result
  readonly submitting = signal(false);
  readonly result = signal<BulkAllotmentResult | null>(null);

  readonly canSubmit = computed(() => {
    if (this.submitting()) return false;
    if (!this.leaveTypeId() || this.allocatedDays() <= 0) return false;
    if (this.scope() === 'branch') return !!this.branchId();
    return this.selectedIds().size > 0;
  });

  readonly targetCount = computed(() => {
    if (this.scope() === 'branch') return this.employees$().length;
    return this.selectedIds().size;
  });

  ngOnInit(): void {
    this.leaveTypes.getAll({ pageSize: 100, isActive: true }).subscribe({
      next: (res) => {
        if (res.success && res.data) this.leaveTypeOptions.set(res.data.items);
      },
    });
    this.branches.getAll({ pageSize: 200, isActive: true }).subscribe({
      next: (res) => {
        if (res.success && res.data) this.branchOptions.set(res.data.items);
      },
    });
    this.loadEmployees();
  }

  setScope(scope: Scope): void {
    this.scope.set(scope);
    this.selectedIds.set(new Set());
    this.loadEmployees();
  }

  onBranchChange(value: number | null): void {
    this.branchId.set(value);
    this.loadEmployees();
  }

  loadEmployees(): void {
    this.loadingEmployees.set(true);
    const branchId = this.scope() === 'branch' ? this.branchId() ?? undefined : undefined;
    this.employees
      .getAll({ branchId, status: 'Active', pageSize: 200 })
      .subscribe({
        next: (res) => {
          this.loadingEmployees.set(false);
          if (res.success && res.data) this.employees$.set(res.data.items);
        },
        error: () => this.loadingEmployees.set(false),
      });
  }

  toggleEmployee(id: number): void {
    const next = new Set(this.selectedIds());
    if (next.has(id)) next.delete(id);
    else next.add(id);
    this.selectedIds.set(next);
  }

  isSelected(id: number): boolean {
    return this.selectedIds().has(id);
  }

  selectAll(): void {
    this.selectedIds.set(new Set(this.employees$().map((e) => e.id)));
  }

  clearSelection(): void {
    this.selectedIds.set(new Set());
  }

  submit(): void {
    if (!this.canSubmit() || this.submitting()) return;

    this.submitting.set(true);
    this.result.set(null);

    this.service
      .bulkCreate({
        year: this.year(),
        leaveTypeId: this.leaveTypeId()!,
        allocatedDays: this.allocatedDays(),
        branchId: this.scope() === 'branch' ? this.branchId() ?? undefined : undefined,
        employeeIds: this.scope() === 'manual' ? Array.from(this.selectedIds()) : undefined,
      })
      .subscribe({
        next: (res) => {
          this.submitting.set(false);
          if (res.success && res.data) {
            this.result.set(res.data);
            this.toast.success('Bulk allotment completed.');
          } else {
            this.toast.error(res.message || 'Bulk allotment failed.');
          }
        },
        error: (err: HttpErrorResponse) => {
          this.submitting.set(false);
          this.toast.error(err.error?.message || 'Bulk allotment failed.');
        },
      });
  }

  startOver(): void {
    this.result.set(null);
  }

  goToList(): void {
    this.router.navigate(['/leave/allotments']);
  }
}
