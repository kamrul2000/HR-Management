import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import {
  Component,
  EventEmitter,
  Input,
  OnChanges,
  OnInit,
  Output,
  SimpleChanges,
  inject,
  signal,
} from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';

import { ToastService } from '../../../../core/services/toast.service';
import { DrawerComponent } from '../../../../shared/components/drawer/drawer.component';
import { EmployeeResponse } from '../../../employee/models/employee.model';
import { EmployeeService } from '../../../employee/services/employee.service';
import {
  CreateLeaveAllotmentDto,
  LeaveAllotmentResponse,
  UpdateLeaveAllotmentDto,
} from '../../models/leave-allotment.model';
import { LeaveTypeResponse } from '../../models/leave-type.model';
import { LeaveAllotmentService } from '../../services/leave-allotment.service';
import { LeaveTypeService } from '../../services/leave-type.service';

@Component({
  selector: 'hrm-allotment-form',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, DrawerComponent],
  templateUrl: './allotment-form.component.html',
  styles: [
    `
      .emp-autocomplete {
        position: absolute;
        z-index: 5;
        background: #fff;
        border: 1px solid #E2E8F0;
        border-radius: 8px;
        box-shadow: 0 4px 6px rgba(0,0,0,.07);
        margin-top: 4px;
        width: 100%;
        max-height: 240px;
        overflow-y: auto;
        list-style: none;
        padding: 4px;
        font-size: 13px;
      }
      .emp-autocomplete li {
        padding: 8px 10px;
        border-radius: 4px;
        cursor: pointer;
      }
      .emp-autocomplete li:hover { background: #F1F5F9; }
    `,
  ],
})
export class AllotmentFormComponent implements OnInit, OnChanges {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(LeaveAllotmentService);
  private readonly leaveTypes = inject(LeaveTypeService);
  private readonly employees = inject(EmployeeService);
  private readonly toast = inject(ToastService);

  @Input() allotment: LeaveAllotmentResponse | null = null;

  @Output() saved = new EventEmitter<LeaveAllotmentResponse>();
  @Output() dismiss = new EventEmitter<void>();

  readonly form = this.fb.nonNullable.group({
    employeeId: [0, [Validators.required, Validators.min(1)]],
    employeeLabel: [''],
    leaveTypeId: [0, [Validators.required, Validators.min(1)]],
    year: [new Date().getFullYear(), [Validators.required, Validators.min(2000), Validators.max(2100)]],
    allocatedDays: [0, [Validators.required, Validators.min(0), Validators.max(365)]],
    carriedForwardDays: [0, [Validators.min(0), Validators.max(365)]],
    isActive: [true],
  });

  readonly saving = signal(false);
  readonly leaveTypeOptions = signal<LeaveTypeResponse[]>([]);
  readonly searchResults = signal<EmployeeResponse[]>([]);
  private readonly search$ = new Subject<string>();

  get isEdit(): boolean { return this.allotment !== null; }

  ngOnInit(): void {
    this.leaveTypes.getAll({ pageSize: 100, isActive: true }).subscribe({
      next: (res) => {
        if (res.success && res.data) this.leaveTypeOptions.set(res.data.items);
      },
    });

    this.search$
      .pipe(debounceTime(300), distinctUntilChanged())
      .subscribe((term) => this.runSearch(term));
  }

  ngOnChanges(changes: SimpleChanges): void {
    if ('allotment' in changes) {
      if (this.allotment) {
        this.form.patchValue({
          employeeId: this.allotment.employeeId,
          employeeLabel: `${this.allotment.employeeFullName ?? ''} (${this.allotment.employeeCode ?? ''})`,
          leaveTypeId: this.allotment.leaveTypeId,
          year: this.allotment.year,
          allocatedDays: this.allotment.allocatedDays,
          carriedForwardDays: this.allotment.carriedForwardDays,
          isActive: this.allotment.isActive,
        });
      } else {
        this.form.reset({
          employeeId: 0,
          employeeLabel: '',
          leaveTypeId: 0,
          year: new Date().getFullYear(),
          allocatedDays: 0,
          carriedForwardDays: 0,
          isActive: true,
        });
      }
    }
  }

  onEmployeeSearch(term: string): void {
    this.form.patchValue({ employeeLabel: term, employeeId: 0 });
    this.search$.next(term.trim());
  }

  private runSearch(term: string): void {
    if (!term) {
      this.searchResults.set([]);
      return;
    }
    this.employees.getAll({ search: term, status: 'Active', pageSize: 8 }).subscribe({
      next: (res) => {
        if (res.success && res.data) this.searchResults.set(res.data.items);
      },
    });
  }

  pickEmployee(emp: EmployeeResponse): void {
    this.form.patchValue({
      employeeId: emp.id,
      employeeLabel: `${emp.fullName} (${emp.employeeCode})`,
    });
    this.searchResults.set([]);
  }

  submit(): void {
    if (this.form.invalid || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }
    const raw = this.form.getRawValue();

    this.saving.set(true);
    if (this.isEdit && this.allotment) {
      const dto: UpdateLeaveAllotmentDto = {
        allocatedDays: raw.allocatedDays,
        carriedForwardDays: raw.carriedForwardDays,
        isActive: raw.isActive,
      };
      this.service.update(this.allotment.id, dto).subscribe({
        next: (res) => this.handleResult(res),
        error: (err: HttpErrorResponse) => {
          this.saving.set(false);
          this.toast.error(err.error?.message || 'Failed to save allotment.');
        },
      });
    } else {
      const dto: CreateLeaveAllotmentDto = {
        employeeId: raw.employeeId,
        leaveTypeId: raw.leaveTypeId,
        year: raw.year,
        allocatedDays: raw.allocatedDays,
        carriedForwardDays: raw.carriedForwardDays,
      };
      this.service.create(dto).subscribe({
        next: (res) => this.handleResult(res),
        error: (err: HttpErrorResponse) => {
          this.saving.set(false);
          this.toast.error(err.error?.message || 'Failed to save allotment.');
        },
      });
    }
  }

  private handleResult(res: { success: boolean; message: string; data: LeaveAllotmentResponse | null }): void {
    this.saving.set(false);
    if (res.success && res.data) {
      this.toast.success(this.isEdit ? 'Allotment updated.' : 'Allotment created.');
      this.saved.emit(res.data);
    } else {
      this.toast.error(res.message || 'Failed to save allotment.');
    }
  }

  hasError(field: keyof typeof this.form.controls, error: string): boolean {
    const ctrl = this.form.controls[field];
    return ctrl.touched && ctrl.hasError(error);
  }
}
