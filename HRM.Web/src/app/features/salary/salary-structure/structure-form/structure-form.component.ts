import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { heroArrowLeft, heroCheck } from '@ng-icons/heroicons/outline';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';

import { ToastService } from '../../../../core/services/toast.service';
import { LoadingSkeletonComponent } from '../../../../shared/components/loading-skeleton/loading-skeleton.component';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import { CurrencyBdPipe } from '../../../../shared/pipes/currency-bd.pipe';
import { EmployeeResponse } from '../../../employee/models/employee.model';
import { EmployeeService } from '../../../employee/services/employee.service';
import { SalaryHeadResponse } from '../../models/salary-head.model';
import {
  CreateSalaryStructureDto,
  SalaryStructureItemDto,
  UpdateSalaryStructureDto,
} from '../../models/salary-structure.model';
import { SalaryHeadService } from '../../services/salary-head.service';
import { SalaryStructureService } from '../../services/salary-structure.service';

interface BuilderItem {
  head: SalaryHeadResponse;
  selected: boolean;
  fixedAmount: number | null;
  overridePercentage: number | null;
}

@Component({
  selector: 'hrm-structure-form',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterLink,
    NgIcon,
    PageHeaderComponent,
    LoadingSkeletonComponent,
    CurrencyBdPipe,
  ],
  providers: [provideIcons({ heroArrowLeft, heroCheck })],
  templateUrl: './structure-form.component.html',
  styleUrl: './structure-form.component.scss',
})
export class StructureFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(SalaryStructureService);
  private readonly heads = inject(SalaryHeadService);
  private readonly employees = inject(EmployeeService);
  private readonly toast = inject(ToastService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly editingId = signal<number | null>(null);
  readonly loading = signal(false);
  readonly saving = signal(false);

  readonly form = this.fb.nonNullable.group({
    employeeId: [0, [Validators.required, Validators.min(1)]],
    employeeLabel: [''],
    effectiveFrom: ['', [Validators.required]],
    basicSalary: [0, [Validators.required, Validators.min(0.01)]],
    remarks: [''],
  });

  readonly searchResults = signal<EmployeeResponse[]>([]);
  readonly items = signal<BuilderItem[]>([]);
  private readonly search$ = new Subject<string>();

  readonly isEdit = computed(() => this.editingId() !== null);

  // ── Live preview ────────────────────────────────────────
  readonly preview = computed(() => {
    const basic = Number(this.form.value.basicSalary) || 0;
    const selected = this.items().filter((i) => i.selected);

    let earnings = basic;
    let deductions = 0;

    // Earnings (excluding the implicit basic)
    for (const item of selected) {
      if (item.head.headType !== 'Earning') continue;
      earnings += this.computeAmount(item, basic, earnings);
    }

    // Deductions — computed against the running earnings
    for (const item of selected) {
      if (item.head.headType !== 'Deduction') continue;
      deductions += this.computeAmount(item, basic, earnings);
    }

    return {
      gross: roundTo(earnings, 2),
      deductions: roundTo(deductions, 2),
      net: roundTo(earnings - deductions, 2),
    };
  });

  ngOnInit(): void {
    this.heads.getAll({ pageSize: 200, isActive: true }).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          const sorted = [...res.data.items].sort((a, b) =>
            a.headType === b.headType
              ? a.displayOrder - b.displayOrder
              : a.headType === 'Earning' ? -1 : 1,
          );
          this.items.set(
            sorted.map((h) => ({
              head: h,
              selected: false,
              fixedAmount: null,
              overridePercentage: null,
            })),
          );

          const idParam = this.route.snapshot.paramMap.get('id');
          if (idParam) this.loadStructure(Number(idParam));
        }
      },
    });

    this.search$.pipe(debounceTime(300), distinctUntilChanged()).subscribe((term) => this.runEmployeeSearch(term));
  }

  private loadStructure(id: number): void {
    this.editingId.set(id);
    this.loading.set(true);
    this.service.getById(id).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) {
          const s = res.data;
          this.form.patchValue({
            employeeId: s.employeeId,
            employeeLabel: `${s.employeeFullName ?? ''} (${s.employeeCode ?? ''})`,
            effectiveFrom: s.effectiveFrom?.slice(0, 10),
            basicSalary: s.basicSalary,
            remarks: s.remarks ?? '',
          });
          // Mark selected items + populate amounts
          this.items.set(this.items().map((bi) => {
            const found = s.items.find((it) => it.salaryHeadId === bi.head.id);
            return found
              ? {
                  ...bi,
                  selected: true,
                  fixedAmount: found.fixedAmount ?? null,
                  overridePercentage: found.overridePercentage ?? null,
                }
              : bi;
          }));
        }
      },
      error: () => this.loading.set(false),
    });
  }

  // ── Employee picker ─────────────────────────────────────
  onEmployeeSearch(term: string): void {
    this.form.patchValue({ employeeLabel: term, employeeId: 0 });
    this.search$.next(term.trim());
  }

  private runEmployeeSearch(term: string): void {
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

  // ── Item builder ────────────────────────────────────────
  toggleItem(headId: number): void {
    this.items.set(this.items().map((bi) =>
      bi.head.id === headId ? { ...bi, selected: !bi.selected } : bi,
    ));
  }

  setItemAmount(headId: number, value: number): void {
    this.items.set(this.items().map((bi) =>
      bi.head.id === headId ? { ...bi, fixedAmount: value || null } : bi,
    ));
  }

  setItemPercent(headId: number, value: number): void {
    this.items.set(this.items().map((bi) =>
      bi.head.id === headId ? { ...bi, overridePercentage: value || null } : bi,
    ));
  }

  earnings = computed(() => this.items().filter((i) => i.head.headType === 'Earning'));
  deductions = computed(() => this.items().filter((i) => i.head.headType === 'Deduction'));

  // ── Submit ──────────────────────────────────────────────
  submit(): void {
    if (this.form.invalid || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const itemsPayload: SalaryStructureItemDto[] = this.items()
      .filter((i) => i.selected)
      .map((i) => ({
        salaryHeadId: i.head.id,
        fixedAmount: i.head.calculationMethod === 'Fixed' ? i.fixedAmount : null,
        overridePercentage: i.head.calculationMethod !== 'Fixed' ? i.overridePercentage : null,
      }));

    if (itemsPayload.length === 0) {
      this.toast.error('Select at least one salary head.');
      return;
    }

    this.saving.set(true);
    if (this.isEdit() && this.editingId()) {
      const dto: UpdateSalaryStructureDto = {
        remarks: raw.remarks?.trim() || null,
        items: itemsPayload,
      };
      this.service.update(this.editingId()!, dto).subscribe({
        next: (res) => this.handleResult(res, 'Salary structure updated.'),
        error: (err: HttpErrorResponse) => this.handleError(err),
      });
    } else {
      const dto: CreateSalaryStructureDto = {
        employeeId: raw.employeeId,
        effectiveFrom: raw.effectiveFrom,
        basicSalary: raw.basicSalary,
        remarks: raw.remarks?.trim() || null,
        items: itemsPayload,
      };
      this.service.create(dto).subscribe({
        next: (res) => this.handleResult(res, 'Salary structure created.'),
        error: (err: HttpErrorResponse) => this.handleError(err),
      });
    }
  }

  private handleResult<T>(res: { success: boolean; message: string; data: T | null }, msg: string): void {
    this.saving.set(false);
    if (res.success) {
      this.toast.success(msg);
      this.router.navigate(['/salary/structures']);
    } else {
      this.toast.error(res.message || 'Failed to save structure.');
    }
  }

  private handleError(err: HttpErrorResponse): void {
    this.saving.set(false);
    this.toast.error(err.error?.message || 'Failed to save structure.');
  }

  hasError(field: keyof typeof this.form.controls, error: string): boolean {
    const ctrl = this.form.controls[field];
    return ctrl.touched && ctrl.hasError(error);
  }

  // ── helpers ─────────────────────────────────────────────
  private computeAmount(item: BuilderItem, basic: number, runningGross: number): number {
    const head = item.head;
    if (head.calculationMethod === 'Fixed') return Number(item.fixedAmount) || 0;
    const pct = item.overridePercentage ?? head.percentage ?? 0;
    if (!pct) return 0;
    if (head.calculationMethod === 'PercentageOfBasic') return (basic * pct) / 100;
    if (head.calculationMethod === 'PercentageOfGross') return (runningGross * pct) / 100;
    return 0; // PercentageOfNet / PercentageOfHead — server-only
  }
}

function roundTo(value: number, decimals: number): number {
  const f = Math.pow(10, decimals);
  return Math.round(value * f) / f;
}
