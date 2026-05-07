import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import {
  Component,
  EventEmitter,
  OnInit,
  Output,
  computed,
  inject,
  signal,
} from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';

import { ToastService } from '../../../../core/services/toast.service';
import { DrawerComponent } from '../../../../shared/components/drawer/drawer.component';
import { EmployeeResponse } from '../../../employee/models/employee.model';
import { EmployeeService } from '../../../employee/services/employee.service';
import { CreateSeparationDto } from '../../models/employee-separation.model';
import { SeparationReasonResponse, SeparationType } from '../../models/separation-reason.model';
import { EmployeeSeparationService } from '../../services/employee-separation.service';
import { SeparationReasonService } from '../../services/separation-reason.service';

const TYPE_OPTIONS: SeparationType[] = ['Resignation', 'Termination', 'Retirement', 'Death', 'Contract End'];

@Component({
  selector: 'hrm-separation-form',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, DrawerComponent],
  templateUrl: './separation-form.component.html',
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
      .emp-autocomplete li { padding: 8px 10px; border-radius: 4px; cursor: pointer; }
      .emp-autocomplete li:hover { background: #F1F5F9; }
    `,
  ],
})
export class SeparationFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(EmployeeSeparationService);
  private readonly reasons = inject(SeparationReasonService);
  private readonly employees = inject(EmployeeService);
  private readonly toast = inject(ToastService);

  @Output() saved = new EventEmitter<void>();
  @Output() dismiss = new EventEmitter<void>();

  readonly typeOptions = TYPE_OPTIONS;

  readonly form = this.fb.nonNullable.group({
    employeeId: [0, [Validators.required, Validators.min(1)]],
    employeeLabel: [''],
    separationType: ['Resignation' as SeparationType, [Validators.required]],
    separationReasonId: [0, [Validators.required, Validators.min(1)]],
    applicationDate: [new Date().toISOString().slice(0, 10), [Validators.required]],
    lastWorkingDate: ['', [Validators.required]],
    noticePeriodDays: [60, [Validators.required, Validators.min(0), Validators.max(365)]],
    noticePeriodBuyout: [0, [Validators.min(0)]],
    otherSettlementAmount: [0, [Validators.min(0)]],
    remarks: [''],
  });

  readonly saving = signal(false);
  readonly searchResults = signal<EmployeeResponse[]>([]);
  readonly availableReasons = signal<SeparationReasonResponse[]>([]);
  private readonly search$ = new Subject<string>();

  readonly currentType = computed(() => this.form.value.separationType ?? 'Resignation');

  ngOnInit(): void {
    this.search$.pipe(debounceTime(300), distinctUntilChanged()).subscribe((term) => this.runSearch(term));
    this.loadReasons('Resignation');
    this.form.controls.separationType.valueChanges.subscribe((t) => {
      this.form.patchValue({ separationReasonId: 0 });
      this.loadReasons(t);
    });
  }

  private loadReasons(type: SeparationType): void {
    this.reasons.getByType(type).subscribe({
      next: (res) => {
        if (res.success && res.data) this.availableReasons.set(res.data);
      },
    });
  }

  onEmployeeSearch(term: string): void {
    this.form.patchValue({ employeeLabel: term, employeeId: 0 });
    this.search$.next(term.trim());
  }

  private runSearch(term: string): void {
    if (!term) { this.searchResults.set([]); return; }
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
    if (this.saving()) return;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const raw = this.form.getRawValue();
    const dto: CreateSeparationDto = {
      employeeId: raw.employeeId,
      separationReasonId: +raw.separationReasonId,
      separationType: raw.separationType,
      applicationDate: raw.applicationDate,
      lastWorkingDate: raw.lastWorkingDate,
      noticePeriodDays: +raw.noticePeriodDays,
      noticePeriodBuyout: +raw.noticePeriodBuyout,
      otherSettlementAmount: +raw.otherSettlementAmount,
      remarks: raw.remarks?.trim() || null,
    };
    this.saving.set(true);
    this.service.create(dto).subscribe({
      next: (res) => {
        this.saving.set(false);
        if (res.success) {
          this.toast.success('Separation initiated.');
          this.saved.emit();
        } else {
          this.toast.error(res.message || 'Failed to save.');
        }
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.toast.error(err.error?.message || 'Failed to save.');
      },
    });
  }
}
