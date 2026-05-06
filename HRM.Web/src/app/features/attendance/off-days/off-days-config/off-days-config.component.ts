import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { heroCheck, heroBuildingOffice2, heroGlobeAlt } from '@ng-icons/heroicons/outline';

import { ToastService } from '../../../../core/services/toast.service';
import { LoadingSkeletonComponent } from '../../../../shared/components/loading-skeleton/loading-skeleton.component';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import { BranchResponse } from '../../../organization/models/branch.model';
import { BranchService } from '../../../organization/services/branch.service';
import { DAY_NAMES } from '../../models/off-day.model';
import { OffDayService } from '../../services/off-day.service';

type Scope = 'org' | 'branch';

@Component({
  selector: 'hrm-off-days-config',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    NgIcon,
    PageHeaderComponent,
    LoadingSkeletonComponent,
  ],
  providers: [provideIcons({ heroCheck, heroBuildingOffice2, heroGlobeAlt })],
  templateUrl: './off-days-config.component.html',
  styleUrl: './off-days-config.component.scss',
})
export class OffDaysConfigComponent implements OnInit {
  private readonly service = inject(OffDayService);
  private readonly branches = inject(BranchService);
  private readonly toast = inject(ToastService);

  readonly dayNames = DAY_NAMES;
  readonly scope = signal<Scope>('org');
  readonly branchOptions = signal<BranchResponse[]>([]);
  readonly selectedBranchId = signal<number | null>(null);
  readonly selectedDays = signal<Set<number>>(new Set());

  readonly loading = signal(true);
  readonly saving = signal(false);

  readonly canSave = computed(() => {
    if (this.loading()) return false;
    return this.scope() === 'org' || this.selectedBranchId() !== null;
  });

  ngOnInit(): void {
    this.loadBranches();
    this.load();
  }

  loadBranches(): void {
    this.branches.getAll({ pageSize: 200, isActive: true }).subscribe({
      next: (res) => {
        if (res.success && res.data) this.branchOptions.set(res.data.items);
      },
    });
  }

  setScope(scope: Scope): void {
    this.scope.set(scope);
    if (scope === 'org') this.selectedBranchId.set(null);
    this.load();
  }

  onBranchChange(value: number | null): void {
    this.selectedBranchId.set(value);
    this.load();
  }

  toggleDay(day: number): void {
    const current = new Set(this.selectedDays());
    if (current.has(day)) current.delete(day);
    else current.add(day);
    this.selectedDays.set(current);
  }

  isSelected(day: number): boolean {
    return this.selectedDays().has(day);
  }

  load(): void {
    if (this.scope() === 'branch' && !this.selectedBranchId()) {
      this.selectedDays.set(new Set());
      this.loading.set(false);
      return;
    }

    this.loading.set(true);
    const branchId = this.scope() === 'org' ? null : this.selectedBranchId();
    this.service.getAll({ branchId }).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.selectedDays.set(new Set(res.data.filter((d) => d.isActive).map((d) => d.dayOfWeek)));
        }
      },
      error: () => this.loading.set(false),
    });
  }

  save(): void {
    if (!this.canSave() || this.saving()) return;

    const branchId = this.scope() === 'org' ? null : this.selectedBranchId();
    this.saving.set(true);
    this.service
      .bulkSet({
        branchId,
        daysOfWeek: Array.from(this.selectedDays()).sort(),
      })
      .subscribe({
        next: (res) => {
          this.saving.set(false);
          if (res.success) {
            const labels = Array.from(this.selectedDays())
              .sort()
              .map((d) => this.dayNames[d])
              .join(', ');
            this.toast.success(
              labels.length ? `Off days set: ${labels}.` : 'All days are marked as working days.',
            );
          } else {
            this.toast.error(res.message || 'Failed to save off days.');
          }
        },
        error: (err: HttpErrorResponse) => {
          this.saving.set(false);
          this.toast.error(err.error?.message || 'Failed to save off days.');
        },
      });
  }
}
