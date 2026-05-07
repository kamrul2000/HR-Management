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
import { heroPlus, heroPlay, heroDocumentMagnifyingGlass } from '@ng-icons/heroicons/outline';

import { ConfirmService } from '../../../../core/services/confirm.service';
import { ToastService } from '../../../../core/services/toast.service';
import { AvatarComponent } from '../../../../shared/components/avatar/avatar.component';
import { DataTableComponent } from '../../../../shared/components/data-table/data-table.component';
import { TableColumn } from '../../../../shared/components/data-table/data-table.types';
import { DrawerComponent } from '../../../../shared/components/drawer/drawer.component';
import { LoadingSkeletonComponent } from '../../../../shared/components/loading-skeleton/loading-skeleton.component';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';
import { CurrencyBdPipe } from '../../../../shared/pipes/currency-bd.pipe';
import {
  CreatePfInterestRateDto,
  EmployeePfInterestResponse,
  PfInterestRateResponse,
  PfInterestReport,
} from '../../models/pf-interest.model';
import { PfInterestService } from '../../services/pf-interest.service';

@Component({
  selector: 'hrm-pf-interest-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    NgIcon,
    PageHeaderComponent,
    DataTableComponent,
    AvatarComponent,
    StatusBadgeComponent,
    CurrencyBdPipe,
    DrawerComponent,
    LoadingSkeletonComponent,
  ],
  providers: [provideIcons({ heroPlus, heroPlay, heroDocumentMagnifyingGlass })],
  templateUrl: './interest-list.component.html',
  styles: [
    `
      .stat-strip {
        display: grid;
        grid-template-columns: repeat(4, 1fr);
        gap: 12px;
        margin-bottom: 16px;
      }
      .stat-card {
        background: #fff;
        border: 1px solid #E2E8F0;
        border-radius: 8px;
        padding: 14px 16px;
      }
      .stat-card__label { font-size: 12px; color: #64748B; text-transform: uppercase; letter-spacing: 0.4px; }
      .stat-card__value { font-weight: 600; color: #0F172A; font-size: 18px; margin-top: 4px; }
      .stat-card--interest .stat-card__value { color: #16A34A; }
      .stat-card--closing  .stat-card__value { color: #2563EB; }

      .rate-card {
        border: 1px solid #E2E8F0;
        border-radius: 8px;
        padding: 12px 14px;
        margin-bottom: 8px;
        display: flex;
        align-items: center;
        gap: 12px;
      }
      .rate-card__year { font-weight: 600; font-size: 14px; min-width: 100px; }
      .rate-card__rate { font-weight: 600; color: #16A34A; font-size: 16px; }
      .rate-card__meta { color: #64748B; font-size: 12px; flex: 1; }
    `,
  ],
})
export class InterestListComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(PfInterestService);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmService);

  readonly rates = signal<PfInterestRateResponse[]>([]);
  readonly report = signal<PfInterestReport | null>(null);
  readonly loading = signal(true);
  readonly busy = signal(false);

  readonly fiscalYear = signal<string>('');

  readonly ratesOpen = signal(false);
  readonly addingRate = signal(false);
  readonly savingRate = signal(false);

  readonly rateForm = this.fb.nonNullable.group({
    fiscalYear: ['', [Validators.required, Validators.maxLength(10)]],
    interestRate: [10, [Validators.required, Validators.min(0.01), Validators.max(30)]],
    effectiveFrom: [new Date().toISOString().slice(0, 10), [Validators.required]],
    description: [''],
  });

  @ViewChild('employeeCellTpl', { static: true }) employeeCellTpl!: TemplateRef<{ $implicit: EmployeePfInterestResponse }>;
  @ViewChild('openingTpl',      { static: true }) openingTpl!:      TemplateRef<{ $implicit: EmployeePfInterestResponse }>;
  @ViewChild('contribTpl',      { static: true }) contribTpl!:      TemplateRef<{ $implicit: EmployeePfInterestResponse }>;
  @ViewChild('interestTpl',     { static: true }) interestTpl!:     TemplateRef<{ $implicit: EmployeePfInterestResponse }>;
  @ViewChild('closingTpl',      { static: true }) closingTpl!:      TemplateRef<{ $implicit: EmployeePfInterestResponse }>;

  columns: TableColumn<EmployeePfInterestResponse>[] = [];

  readonly currentFiscalYear = computed(() => {
    const fy = this.fiscalYear();
    if (fy) return fy;
    const now = new Date();
    const y = now.getFullYear();
    return now.getMonth() >= 6 ? `${y}-${y + 1}` : `${y - 1}-${y}`;
  });

  ngOnInit(): void {
    this.fiscalYear.set(this.currentFiscalYear());
    this.columns = [
      { key: 'employee', label: 'Employee', template: this.employeeCellTpl },
      { key: 'rate',    label: 'Rate', width: '90px' },
      { key: 'opening', label: 'Opening', template: this.openingTpl, align: 'right', width: '130px' },
      { key: 'contrib', label: 'Contributions', template: this.contribTpl, align: 'right', width: '150px' },
      { key: 'interest',label: 'Interest', template: this.interestTpl, align: 'right', width: '130px' },
      { key: 'closing', label: 'Closing', template: this.closingTpl, align: 'right', width: '140px' },
    ];
    this.loadRates();
    this.loadReport();
  }

  loadRates(): void {
    this.service.getAllRates().subscribe({
      next: (res) => {
        if (res.success && res.data) this.rates.set(res.data);
      },
    });
  }

  loadReport(): void {
    this.loading.set(true);
    this.service.getReport(this.fiscalYear()).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) this.report.set(res.data);
        else this.report.set(null);
      },
      error: () => {
        this.loading.set(false);
        this.report.set(null);
      },
    });
  }

  onFiscalYearChange(): void {
    this.loadReport();
  }

  bulkCompute(): void {
    if (this.busy()) return;
    this.confirm.confirm({
      title: 'Compute interest',
      message: `Compute PF interest for all eligible employees in fiscal year ${this.fiscalYear()}?`,
      confirmLabel: 'Run Computation',
    }).subscribe((ok) => {
      if (!ok) return;
      this.busy.set(true);
      this.service.bulkCompute({ fiscalYear: this.fiscalYear() }).subscribe({
        next: (res) => {
          this.busy.set(false);
          if (res.success && res.data) {
            const r = res.data;
            this.toast.success(`Computed: ${r.successCount} created, ${r.skippedCount} skipped, ${r.failedCount} failed.`);
            this.loadReport();
          } else {
            this.toast.error(res.message || 'Computation failed.');
          }
        },
        error: (err: HttpErrorResponse) => {
          this.busy.set(false);
          this.toast.error(err.error?.message || 'Computation failed.');
        },
      });
    });
  }

  openRates(): void { this.ratesOpen.set(true); this.addingRate.set(false); }
  closeRates(): void { this.ratesOpen.set(false); }
  startAddRate(): void {
    this.rateForm.reset({
      fiscalYear: this.fiscalYear(),
      interestRate: 10,
      effectiveFrom: new Date().toISOString().slice(0, 10),
      description: '',
    });
    this.addingRate.set(true);
  }
  cancelAddRate(): void { this.addingRate.set(false); }

  saveRate(): void {
    if (this.savingRate()) return;
    if (this.rateForm.invalid) {
      this.rateForm.markAllAsTouched();
      return;
    }
    const raw = this.rateForm.getRawValue();
    const dto: CreatePfInterestRateDto = {
      fiscalYear: raw.fiscalYear.trim(),
      interestRate: +raw.interestRate,
      effectiveFrom: raw.effectiveFrom,
      description: raw.description?.trim() || null,
    };
    this.savingRate.set(true);
    this.service.createRate(dto).subscribe({
      next: (res) => {
        this.savingRate.set(false);
        if (res.success) {
          this.toast.success('Interest rate created.');
          this.addingRate.set(false);
          this.loadRates();
        } else {
          this.toast.error(res.message || 'Save failed.');
        }
      },
      error: (err: HttpErrorResponse) => {
        this.savingRate.set(false);
        this.toast.error(err.error?.message || 'Save failed.');
      },
    });
  }
}
