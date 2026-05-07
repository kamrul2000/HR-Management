import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { heroArrowLeft, heroChevronLeft, heroChevronRight } from '@ng-icons/heroicons/outline';

import { ToastService } from '../../../core/services/toast.service';
import { AvatarComponent } from '../../../shared/components/avatar/avatar.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { LoadingSkeletonComponent } from '../../../shared/components/loading-skeleton/loading-skeleton.component';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { BranchResponse } from '../../organization/models/branch.model';
import { BranchService } from '../../organization/services/branch.service';
import { OvertimeSummary } from '../models/overtime.model';
import { OvertimeService } from '../services/overtime.service';

@Component({
  selector: 'hrm-overtime-summary',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    NgIcon,
    PageHeaderComponent,
    LoadingSkeletonComponent,
    EmptyStateComponent,
    AvatarComponent,
  ],
  providers: [provideIcons({ heroArrowLeft, heroChevronLeft, heroChevronRight })],
  templateUrl: './overtime-summary.component.html',
  styleUrl: './overtime-summary.component.scss',
})
export class OvertimeSummaryComponent implements OnInit {
  private readonly service = inject(OvertimeService);
  private readonly branches = inject(BranchService);
  private readonly toast = inject(ToastService);

  readonly month = signal<number>(new Date().getMonth() + 1);
  readonly year = signal<number>(new Date().getFullYear());
  readonly branchFilter = signal<number | null>(null);

  readonly branchOptions = signal<BranchResponse[]>([]);
  readonly summaries = signal<OvertimeSummary[]>([]);
  readonly loading = signal(true);

  readonly monthLabel = computed(() => {
    const d = new Date(this.year(), this.month() - 1, 1);
    return d.toLocaleDateString('en-GB', { month: 'long', year: 'numeric' });
  });

  readonly maxMinutes = computed(() => {
    const all = this.summaries();
    if (all.length === 0) return 0;
    return Math.max(...all.map((s) => s.totalApprovedMinutes), 1);
  });

  ngOnInit(): void {
    this.branches.getAll({ pageSize: 200, isActive: true }).subscribe({
      next: (res) => {
        if (res.success && res.data) this.branchOptions.set(res.data.items);
      },
    });
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.service
      .getMonthlySummary(this.year(), this.month(), this.branchFilter() ?? undefined)
      .subscribe({
        next: (res) => {
          this.loading.set(false);
          if (res.success && res.data) {
            this.summaries.set(res.data);
          } else {
            this.summaries.set([]);
          }
        },
        error: () => {
          this.loading.set(false);
          this.summaries.set([]);
        },
      });
  }

  prevMonth(): void {
    let m = this.month() - 1;
    let y = this.year();
    if (m < 1) { m = 12; y--; }
    this.month.set(m);
    this.year.set(y);
    this.load();
  }

  nextMonth(): void {
    let m = this.month() + 1;
    let y = this.year();
    if (m > 12) { m = 1; y++; }
    this.month.set(m);
    this.year.set(y);
    this.load();
  }

  onBranchChange(value: number | null): void {
    this.branchFilter.set(value);
    this.load();
  }

  formatMinutes(minutes: number): string {
    if (!minutes) return '0h';
    const h = Math.floor(minutes / 60);
    const m = minutes % 60;
    if (h && m) return `${h}h ${m}m`;
    if (h) return `${h}h`;
    return `${m}m`;
  }

  pct(value: number): number {
    const max = this.maxMinutes();
    return max > 0 ? (value / max) * 100 : 0;
  }
}
