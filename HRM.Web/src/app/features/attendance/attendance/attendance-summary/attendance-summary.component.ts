import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { heroArrowLeft, heroChevronLeft, heroChevronRight } from '@ng-icons/heroicons/outline';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';

import { ToastService } from '../../../../core/services/toast.service';
import { LoadingSkeletonComponent } from '../../../../shared/components/loading-skeleton/loading-skeleton.component';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import { EmployeeResponse } from '../../../employee/models/employee.model';
import { EmployeeService } from '../../../employee/services/employee.service';
import {
  DailyAttendanceCell,
  MonthlyAttendanceSummary,
} from '../../models/attendance.model';
import { AttendanceService } from '../../services/attendance.service';

interface CalendarCell extends DailyAttendanceCell {
  inMonth: boolean;
}

@Component({
  selector: 'hrm-attendance-summary',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    NgIcon,
    PageHeaderComponent,
    LoadingSkeletonComponent,
  ],
  providers: [provideIcons({ heroArrowLeft, heroChevronLeft, heroChevronRight })],
  templateUrl: './attendance-summary.component.html',
  styleUrl: './attendance-summary.component.scss',
})
export class AttendanceSummaryComponent implements OnInit {
  private readonly attendance = inject(AttendanceService);
  private readonly employees = inject(EmployeeService);
  private readonly toast = inject(ToastService);

  readonly cursor = signal(startOfMonth(new Date()));
  readonly selectedEmployee = signal<EmployeeResponse | null>(null);
  readonly summary = signal<MonthlyAttendanceSummary | null>(null);
  readonly loading = signal(false);
  readonly searchTerm = signal('');
  readonly results = signal<EmployeeResponse[]>([]);
  readonly searching = signal(false);

  private readonly search$ = new Subject<string>();

  readonly monthLabel = computed(() =>
    this.cursor().toLocaleDateString('en-GB', { month: 'long', year: 'numeric' }),
  );

  readonly cells = computed<CalendarCell[]>(() => {
    const summary = this.summary();
    return buildMonthGrid(this.cursor(), summary?.daily ?? []);
  });

  ngOnInit(): void {
    this.search$
      .pipe(debounceTime(300), distinctUntilChanged())
      .subscribe((term) => this.runSearch(term));
  }

  onSearchInput(term: string): void {
    this.searchTerm.set(term);
    this.search$.next(term.trim());
  }

  private runSearch(term: string): void {
    if (!term) {
      this.results.set([]);
      return;
    }
    this.searching.set(true);
    this.employees
      .getAll({ search: term, status: 'Active', pageSize: 8 })
      .subscribe({
        next: (res) => {
          this.searching.set(false);
          if (res.success && res.data) this.results.set(res.data.items);
        },
        error: () => this.searching.set(false),
      });
  }

  selectEmployee(emp: EmployeeResponse): void {
    this.selectedEmployee.set(emp);
    this.results.set([]);
    this.searchTerm.set(emp.fullName);
    this.load();
  }

  prevMonth(): void {
    const c = this.cursor();
    this.cursor.set(new Date(c.getFullYear(), c.getMonth() - 1, 1));
    this.load();
  }

  nextMonth(): void {
    const c = this.cursor();
    this.cursor.set(new Date(c.getFullYear(), c.getMonth() + 1, 1));
    this.load();
  }

  load(): void {
    const emp = this.selectedEmployee();
    if (!emp) return;
    const c = this.cursor();
    this.loading.set(true);
    this.attendance
      .getMonthlySummary(emp.id, c.getFullYear(), c.getMonth() + 1)
      .subscribe({
        next: (res) => {
          this.loading.set(false);
          if (res.success && res.data) {
            this.summary.set(res.data);
          } else {
            this.summary.set(null);
            this.toast.error(res.message || 'Could not load summary.');
          }
        },
        error: () => {
          this.loading.set(false);
          this.summary.set(null);
        },
      });
  }
}

function startOfMonth(d: Date): Date {
  return new Date(d.getFullYear(), d.getMonth(), 1);
}

function buildMonthGrid(cursor: Date, daily: DailyAttendanceCell[]): CalendarCell[] {
  const first = startOfMonth(cursor);
  const startDow = first.getDay();
  const daysInMonth = new Date(cursor.getFullYear(), cursor.getMonth() + 1, 0).getDate();

  const lookup = new Map(daily.map((d) => [d.day, d]));
  const cells: CalendarCell[] = [];

  for (let i = startDow - 1; i >= 0; i--) {
    cells.push(emptyCell(0, false));
  }

  for (let day = 1; day <= daysInMonth; day++) {
    const found = lookup.get(day);
    const dateStr = isoDate(new Date(cursor.getFullYear(), cursor.getMonth(), day));
    cells.push({
      day,
      date: found?.date ?? dateStr,
      status: found?.status ?? 'NoRecord',
      isLate: found?.isLate ?? false,
      punchInTimeFormatted: found?.punchInTimeFormatted ?? null,
      punchOutTimeFormatted: found?.punchOutTimeFormatted ?? null,
      inMonth: true,
    });
  }

  while (cells.length % 7 !== 0) {
    cells.push(emptyCell(0, false));
  }

  return cells;
}

function emptyCell(day: number, inMonth: boolean): CalendarCell {
  return {
    day,
    date: '',
    status: 'NoRecord',
    isLate: false,
    inMonth,
  };
}

function isoDate(d: Date): string {
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
}
