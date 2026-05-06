import { CommonModule } from '@angular/common';
import {
  Component,
  OnInit,
  TemplateRef,
  ViewChild,
  computed,
  inject,
  signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgIcon, provideIcons } from '@ng-icons/core';
import {
  heroPlus,
  heroPencilSquare,
  heroTrash,
  heroChevronLeft,
  heroChevronRight,
  heroCalendarDays,
  heroQueueList,
} from '@ng-icons/heroicons/outline';

import { ConfirmService } from '../../../../core/services/confirm.service';
import { ToastService } from '../../../../core/services/toast.service';
import { DataTableComponent } from '../../../../shared/components/data-table/data-table.component';
import { TableColumn } from '../../../../shared/components/data-table/data-table.types';
import { LoadingSkeletonComponent } from '../../../../shared/components/loading-skeleton/loading-skeleton.component';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';
import { HolidayResponse } from '../../models/holiday.model';
import { HolidayService } from '../../services/holiday.service';
import { HolidayFormComponent } from '../holiday-form/holiday-form.component';

type View = 'calendar' | 'list';
type DrawerMode = 'closed' | { mode: 'create'; date?: string } | { mode: 'edit'; holiday: HolidayResponse };

interface CalendarCell {
  date: string;          // YYYY-MM-DD
  day: number;
  inMonth: boolean;
  isToday: boolean;
  holidays: HolidayResponse[];
}

@Component({
  selector: 'hrm-holiday-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    NgIcon,
    PageHeaderComponent,
    LoadingSkeletonComponent,
    DataTableComponent,
    StatusBadgeComponent,
    HolidayFormComponent,
  ],
  providers: [
    provideIcons({
      heroPlus,
      heroPencilSquare,
      heroTrash,
      heroChevronLeft,
      heroChevronRight,
      heroCalendarDays,
      heroQueueList,
    }),
  ],
  templateUrl: './holiday-list.component.html',
  styleUrl: './holiday-list.component.scss',
})
export class HolidayListComponent implements OnInit {
  private readonly service = inject(HolidayService);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmService);

  readonly view = signal<View>('calendar');
  readonly cursor = signal(startOfMonth(new Date()));

  readonly holidays = signal<HolidayResponse[]>([]);
  readonly loading = signal(true);
  readonly drawer = signal<DrawerMode>('closed');

  @ViewChild('typeCellTpl', { static: true })   typeCellTpl!:   TemplateRef<{ $implicit: HolidayResponse }>;
  @ViewChild('scopeCellTpl', { static: true })  scopeCellTpl!:  TemplateRef<{ $implicit: HolidayResponse }>;
  @ViewChild('statusCellTpl', { static: true }) statusCellTpl!: TemplateRef<{ $implicit: HolidayResponse }>;

  columns: TableColumn<HolidayResponse>[] = [];

  readonly monthLabel = computed(() =>
    this.cursor().toLocaleDateString('en-GB', { month: 'long', year: 'numeric' }),
  );

  readonly cells = computed<CalendarCell[]>(() => buildMonthGrid(this.cursor(), this.holidays()));

  ngOnInit(): void {
    this.columns = [
      { key: 'holidayDate', label: 'Date',  width: '140px' },
      { key: 'holidayName', label: 'Name' },
      { key: 'holidayType', label: 'Type',  template: this.typeCellTpl, width: '140px' },
      { key: 'scope',       label: 'Scope', template: this.scopeCellTpl },
      { key: 'isActive',    label: 'Status', template: this.statusCellTpl, align: 'center', width: '110px' },
    ];
    this.load();
  }

  load(): void {
    this.loading.set(true);
    const cursor = this.cursor();
    this.service.getAll({ year: cursor.getFullYear() }).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) this.holidays.set(res.data.items);
      },
      error: () => this.loading.set(false),
    });
  }

  setView(view: View): void { this.view.set(view); }

  prevMonth(): void {
    const c = this.cursor();
    this.cursor.set(new Date(c.getFullYear(), c.getMonth() - 1, 1));
  }

  nextMonth(): void {
    const c = this.cursor();
    this.cursor.set(new Date(c.getFullYear(), c.getMonth() + 1, 1));
  }

  today(): void { this.cursor.set(startOfMonth(new Date())); }

  // Filtered table view shows the current cursor month only (mirrors calendar).
  filteredList = computed(() => {
    const c = this.cursor();
    return this.holidays().filter((h) => {
      const d = new Date(h.holidayDate);
      return d.getFullYear() === c.getFullYear() && d.getMonth() === c.getMonth();
    });
  });

  openCreate(date?: string): void {
    this.drawer.set({ mode: 'create', date });
  }

  openEdit(holiday: HolidayResponse): void {
    this.drawer.set({ mode: 'edit', holiday });
  }

  closeDrawer(): void { this.drawer.set('closed'); }

  onSaved(): void {
    this.closeDrawer();
    this.load();
  }

  delete(holiday: HolidayResponse): void {
    this.confirm
      .confirm({
        title: 'Delete holiday',
        message: `Delete "${holiday.holidayName}"? This cannot be undone.`,
        confirmLabel: 'Delete',
        danger: true,
      })
      .subscribe((ok) => {
        if (!ok) return;
        this.service.delete(holiday.id).subscribe({
          next: (res) => {
            if (res.success) {
              this.toast.success('Holiday deleted.');
              this.holidays.set(this.holidays().filter((h) => h.id !== holiday.id));
            } else {
              this.toast.error(res.message || 'Failed to delete holiday.');
            }
          },
          error: (err) => this.toast.error(err.error?.message || 'Failed to delete holiday.'),
        });
      });
  }

  drawerMode = computed(() => this.drawer());
  defaultDate = computed<string | null>(() => {
    const d = this.drawer();
    return typeof d === 'object' && d.mode === 'create' && d.date ? d.date : null;
  });
  editing = computed<HolidayResponse | null>(() => {
    const d = this.drawer();
    return typeof d === 'object' && d.mode === 'edit' ? d.holiday : null;
  });
  drawerOpen = computed(() => this.drawer() !== 'closed');

  formatType(t: string): string { return t; }
}

function startOfMonth(d: Date): Date {
  return new Date(d.getFullYear(), d.getMonth(), 1);
}

function isSameDay(a: Date, b: Date): boolean {
  return (
    a.getFullYear() === b.getFullYear() &&
    a.getMonth() === b.getMonth() &&
    a.getDate() === b.getDate()
  );
}

function isoDate(d: Date): string {
  const y = d.getFullYear();
  const m = String(d.getMonth() + 1).padStart(2, '0');
  const day = String(d.getDate()).padStart(2, '0');
  return `${y}-${m}-${day}`;
}

function buildMonthGrid(cursor: Date, holidays: HolidayResponse[]): CalendarCell[] {
  const first = startOfMonth(cursor);
  const startDow = first.getDay(); // 0 = Sunday
  const daysInMonth = new Date(cursor.getFullYear(), cursor.getMonth() + 1, 0).getDate();

  const today = new Date();
  const cells: CalendarCell[] = [];

  // Lead-in days from the previous month
  for (let i = startDow - 1; i >= 0; i--) {
    const d = new Date(cursor.getFullYear(), cursor.getMonth(), -i);
    cells.push(buildCell(d, false, today, holidays));
  }

  for (let day = 1; day <= daysInMonth; day++) {
    const d = new Date(cursor.getFullYear(), cursor.getMonth(), day);
    cells.push(buildCell(d, true, today, holidays));
  }

  // Trailing days to complete the final week (always 7 columns)
  while (cells.length % 7 !== 0) {
    const last = cells[cells.length - 1];
    const next = new Date(last.date);
    next.setDate(next.getDate() + 1);
    cells.push(buildCell(next, false, today, holidays));
  }

  return cells;
}

function buildCell(d: Date, inMonth: boolean, today: Date, holidays: HolidayResponse[]): CalendarCell {
  const iso = isoDate(d);
  return {
    date: iso,
    day: d.getDate(),
    inMonth,
    isToday: isSameDay(d, today),
    holidays: holidays.filter((h) => h.holidayDate.slice(0, 10) === iso),
  };
}
