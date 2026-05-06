import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { NgxEchartsDirective } from 'ngx-echarts';
import type { EChartsOption } from 'echarts';

import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { StatCardComponent } from '../../shared/components/stat-card/stat-card.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { LoadingSkeletonComponent } from '../../shared/components/loading-skeleton/loading-skeleton.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { CurrencyBdPipe } from '../../shared/pipes/currency-bd.pipe';
import { DashboardService } from './services/dashboard.service';
import {
  AttendanceSummary,
  DepartmentHeadcount,
  RecentLeaveRow,
  RecentSalaryRow,
} from './models/dashboard.models';

@Component({
  selector: 'hrm-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    NgxEchartsDirective,
    PageHeaderComponent,
    StatCardComponent,
    StatusBadgeComponent,
    LoadingSkeletonComponent,
    EmptyStateComponent,
    CurrencyBdPipe,
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit {
  private readonly service = inject(DashboardService);

  // ── Stat-card state ────────────────────────────────────────────────
  readonly employeeCount = signal<number | null>(null);
  readonly attendance = signal<AttendanceSummary | null>(null);
  readonly pendingLeavesCount = signal<number | null>(null);
  readonly pendingLoansCount = signal<number | null>(null);

  // ── Section state ──────────────────────────────────────────────────
  readonly recentLeaves = signal<RecentLeaveRow[]>([]);
  readonly recentLeavesLoading = signal(true);

  readonly recentSalaries = signal<RecentSalaryRow[]>([]);
  readonly recentSalariesLoading = signal(true);

  readonly departments = signal<DepartmentHeadcount[]>([]);
  readonly departmentsLoading = signal(true);
  readonly attendanceLoading = signal(true);

  // ── Derived chart options ──────────────────────────────────────────
  readonly attendanceChart = computed<EChartsOption>(() =>
    buildAttendanceDonut(this.attendance()),
  );

  readonly departmentChart = computed<EChartsOption>(() =>
    buildDepartmentBar(this.departments()),
  );

  ngOnInit(): void {
    this.loadStatCards();
    this.loadCharts();
    this.loadTables();
  }

  // ── Loaders (separate so a slow endpoint doesn't block the rest) ──
  private loadStatCards(): void {
    this.service
      .getEmployeeCount()
      .subscribe((count) => this.employeeCount.set(count));

    this.service.getPendingLeavesCount().subscribe((count) => {
      this.pendingLeavesCount.set(count);
    });

    this.service.getPendingLoansCount().subscribe((count) => {
      this.pendingLoansCount.set(count);
    });
  }

  private loadCharts(): void {
    this.attendanceLoading.set(true);
    this.service.getTodayAttendanceSummary().subscribe((summary) => {
      this.attendance.set(summary);
      this.attendanceLoading.set(false);
    });

    this.departmentsLoading.set(true);
    this.service.getDepartmentHeadcount().subscribe((rows) => {
      this.departments.set(rows);
      this.departmentsLoading.set(false);
    });
  }

  private loadTables(): void {
    this.recentLeavesLoading.set(true);
    this.service.getPendingLeaves(10).subscribe((rows) => {
      this.recentLeaves.set(rows);
      this.recentLeavesLoading.set(false);
    });

    this.recentSalariesLoading.set(true);
    this.service.getRecentSalaryCalculations(5).subscribe((rows) => {
      this.recentSalaries.set(rows);
      this.recentSalariesLoading.set(false);
    });
  }

  // ── Display helpers used in the template ───────────────────────────
  presentCount(): string {
    const a = this.attendance();
    if (!a) return '—';
    return String(a.present);
  }

  trackById = <T extends { id: number }>(_: number, row: T): number => row.id;
}

// ───────────────────────────────────────────── ECharts builders

function buildAttendanceDonut(summary: AttendanceSummary | null): EChartsOption {
  const entries = summary
    ? [
        { name: 'Present',     value: summary.present,   itemStyle: { color: '#16A34A' } },
        { name: 'Late',        value: summary.late,      itemStyle: { color: '#D97706' } },
        { name: 'Half Day',    value: summary.halfDay,   itemStyle: { color: '#C2410C' } },
        { name: 'Absent',      value: summary.absent,    itemStyle: { color: '#DC2626' } },
        { name: 'Holiday',     value: summary.holiday,   itemStyle: { color: '#7C3AED' } },
        { name: 'Weekly Off',  value: summary.weeklyOff, itemStyle: { color: '#6366F1' } },
      ].filter((e) => e.value > 0)
    : [];

  return {
    tooltip: { trigger: 'item', formatter: '{b}: {c} ({d}%)' },
    legend: {
      bottom: 0,
      itemWidth: 10,
      itemHeight: 10,
      icon: 'circle',
      textStyle: { color: '#475569', fontFamily: 'DM Sans', fontSize: 12 },
    },
    series: [
      {
        type: 'pie',
        radius: ['58%', '78%'],
        center: ['50%', '46%'],
        avoidLabelOverlap: true,
        label: { show: false },
        data: entries.length
          ? entries
          : [{ name: 'No data', value: 1, itemStyle: { color: '#E2E8F0' }, label: { show: false } }],
        emphasis: { scale: true, scaleSize: 6 },
        animationDuration: 600,
      },
    ],
  };
}

function buildDepartmentBar(rows: DepartmentHeadcount[]): EChartsOption {
  return {
    tooltip: {
      trigger: 'axis',
      axisPointer: { type: 'shadow' },
      formatter: (params: unknown) => {
        const arr = params as Array<{ name: string; value: number }>;
        if (!arr || !arr.length) return '';
        return `${arr[0].name}: <strong>${arr[0].value}</strong>`;
      },
    },
    grid: { left: 8, right: 12, top: 16, bottom: 8, containLabel: true },
    xAxis: {
      type: 'value',
      axisLine: { show: false },
      axisTick: { show: false },
      splitLine: { lineStyle: { color: '#F1F5F9' } },
      axisLabel: { color: '#64748B', fontFamily: 'DM Sans' },
    },
    yAxis: {
      type: 'category',
      data: rows.map((r) => r.departmentName),
      axisLine: { show: false },
      axisTick: { show: false },
      axisLabel: { color: '#475569', fontFamily: 'DM Sans', fontSize: 12 },
    },
    series: [
      {
        type: 'bar',
        data: rows.map((r) => r.count),
        barWidth: 14,
        itemStyle: { color: '#2563EB', borderRadius: [0, 4, 4, 0] },
        label: { show: true, position: 'right', color: '#475569', fontFamily: 'DM Sans' },
        animationDuration: 600,
      },
    ],
  };
}
