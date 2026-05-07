import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { NgIcon, provideIcons } from '@ng-icons/core';
import {
  heroPlus,
  heroPencilSquare,
  heroTrash,
  heroArrowUp,
  heroArrowDown,
} from '@ng-icons/heroicons/outline';

import { ConfirmService } from '../../../../core/services/confirm.service';
import { ToastService } from '../../../../core/services/toast.service';
import { LoadingSkeletonComponent } from '../../../../shared/components/loading-skeleton/loading-skeleton.component';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';
import { SalaryHeadResponse } from '../../models/salary-head.model';
import { SalaryHeadService } from '../../services/salary-head.service';
import { SalaryHeadFormComponent } from '../salary-head-form/salary-head-form.component';

type DrawerMode = 'closed' | 'create' | { mode: 'edit'; head: SalaryHeadResponse };

@Component({
  selector: 'hrm-salary-head-list',
  standalone: true,
  imports: [
    CommonModule,
    NgIcon,
    PageHeaderComponent,
    LoadingSkeletonComponent,
    StatusBadgeComponent,
    SalaryHeadFormComponent,
  ],
  providers: [
    provideIcons({ heroPlus, heroPencilSquare, heroTrash, heroArrowUp, heroArrowDown }),
  ],
  templateUrl: './salary-head-list.component.html',
  styleUrl: './salary-head-list.component.scss',
})
export class SalaryHeadListComponent implements OnInit {
  private readonly service = inject(SalaryHeadService);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmService);

  readonly all = signal<SalaryHeadResponse[]>([]);
  readonly loading = signal(true);
  readonly drawer = signal<DrawerMode>('closed');

  readonly earnings = computed(() =>
    this.all()
      .filter((h) => h.headType === 'Earning')
      .sort((a, b) => a.displayOrder - b.displayOrder),
  );
  readonly deductions = computed(() =>
    this.all()
      .filter((h) => h.headType === 'Deduction')
      .sort((a, b) => a.displayOrder - b.displayOrder),
  );

  readonly drawerOpen = computed(() => this.drawer() !== 'closed');
  readonly editing = computed<SalaryHeadResponse | null>(() => {
    const d = this.drawer();
    return typeof d === 'object' ? d.head : null;
  });

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.service.getAll({ pageSize: 200 }).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) this.all.set(res.data.items);
      },
      error: () => this.loading.set(false),
    });
  }

  openCreate(): void { this.drawer.set('create'); }
  openEdit(head: SalaryHeadResponse): void { this.drawer.set({ mode: 'edit', head }); }
  closeDrawer(): void { this.drawer.set('closed'); }

  onSaved(updated: SalaryHeadResponse): void {
    const exists = this.all().some((h) => h.id === updated.id);
    if (exists) {
      this.all.set(this.all().map((h) => (h.id === updated.id ? updated : h)));
    } else {
      this.all.set([updated, ...this.all()]);
    }
    this.closeDrawer();
  }

  delete(head: SalaryHeadResponse): void {
    this.confirm
      .confirm({
        title: 'Delete salary head',
        message: `Delete "${head.headName}"? This may affect existing salary structures.`,
        confirmLabel: 'Delete',
        danger: true,
      })
      .subscribe((ok) => {
        if (!ok) return;
        this.service.delete(head.id).subscribe({
          next: (res) => {
            if (res.success) {
              this.toast.success('Salary head deleted.');
              this.all.set(this.all().filter((h) => h.id !== head.id));
            } else {
              this.toast.error(res.message || 'Failed to delete head.');
            }
          },
          error: (err) => this.toast.error(err.error?.message || 'Failed to delete head.'),
        });
      });
  }

  reorder(head: SalaryHeadResponse, direction: -1 | 1): void {
    const newOrder = head.displayOrder + direction;
    if (newOrder < 1) return;
    this.service.update(head.id, {
      headName: head.headName,
      headCode: head.headCode,
      headType: head.headType,
      calculationMethod: head.calculationMethod,
      percentage: head.percentage ?? null,
      baseHeadId: head.baseHeadId ?? null,
      isFixed: head.isFixed,
      isTaxable: head.isTaxable,
      isProvidentFundApplicable: head.isProvidentFundApplicable,
      displayOrder: newOrder,
      description: head.description ?? null,
      isActive: head.isActive,
    }).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.all.set(this.all().map((h) => (h.id === head.id ? res.data! : h)));
        }
      },
      error: (err) => this.toast.error(err.error?.message || 'Failed to reorder.'),
    });
  }

  describeMethod(h: SalaryHeadResponse): string {
    if (h.calculationMethod === 'Fixed') return 'Fixed amount';
    const pct = h.percentage ?? 0;
    switch (h.calculationMethod) {
      case 'PercentageOfBasic': return `${pct}% of Basic`;
      case 'PercentageOfGross': return `${pct}% of Gross`;
      case 'PercentageOfHead':  return `${pct}% of ${h.baseHeadName ?? 'head'}`;
      case 'PercentageOfNet':   return `${pct}% of Net`;
      default:                  return h.calculationMethod;
    }
  }
}
