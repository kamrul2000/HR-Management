import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  EventEmitter,
  Input,
  Output,
  TemplateRef,
  computed,
  signal,
} from '@angular/core';
import { NgIcon, provideIcons } from '@ng-icons/core';
import {
  heroChevronUpDown,
  heroChevronUp,
  heroChevronDown,
  heroChevronLeft,
  heroChevronRight,
} from '@ng-icons/heroicons/outline';

import { LoadingSkeletonComponent } from '../loading-skeleton/loading-skeleton.component';
import { EmptyStateComponent } from '../empty-state/empty-state.component';
import { PageState, SortState, TableColumn } from './data-table.types';

@Component({
  selector: 'hrm-data-table',
  standalone: true,
  imports: [CommonModule, NgIcon, LoadingSkeletonComponent, EmptyStateComponent],
  providers: [
    provideIcons({
      heroChevronUpDown,
      heroChevronUp,
      heroChevronDown,
      heroChevronLeft,
      heroChevronRight,
    }),
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './data-table.component.html',
  styleUrl: './data-table.component.scss',
})
export class DataTableComponent<T = unknown> {
  @Input({ required: true }) columns: TableColumn<T>[] = [];
  @Input({ required: true }) rows: T[] = [];
  @Input() rowKey: keyof T | ((row: T) => string | number) = 'id' as keyof T;
  @Input() loading = false;

  @Input() page: PageState = { pageNumber: 1, pageSize: 20, totalCount: 0 };
  @Input() showPagination = true;

  @Input() emptyTitle = 'No records found';
  @Input() emptyMessage = 'There is nothing to display here yet.';
  @Input() actionsTemplate?: TemplateRef<{ $implicit: T; row?: T }>;

  @Output() sortChange = new EventEmitter<SortState>();
  @Output() pageChange = new EventEmitter<number>();

  protected readonly sort = signal<SortState | null>(null);

  readonly totalPages = computed(() => {
    if (!this.page || this.page.pageSize <= 0) return 0;
    return Math.max(1, Math.ceil(this.page.totalCount / this.page.pageSize));
  });

  trackRow = (index: number, row: T): string | number => {
    if (typeof this.rowKey === 'function') return this.rowKey(row);
    const value = (row as Record<string, unknown>)[this.rowKey as string];
    return typeof value === 'string' || typeof value === 'number' ? value : index;
  };

  cellValue(row: T, key: string): unknown {
    if (!key.includes('.')) return (row as Record<string, unknown>)[key];
    return key.split('.').reduce<unknown>((acc, part) => {
      if (acc && typeof acc === 'object' && part in (acc as Record<string, unknown>)) {
        return (acc as Record<string, unknown>)[part];
      }
      return undefined;
    }, row);
  }

  toggleSort(column: TableColumn<T>): void {
    if (!column.sortable) return;
    const current = this.sort();
    let next: SortState;
    if (current?.column === column.key) {
      next = { column: column.key, direction: current.direction === 'asc' ? 'desc' : 'asc' };
    } else {
      next = { column: column.key, direction: 'asc' };
    }
    this.sort.set(next);
    this.sortChange.emit(next);
  }

  sortIcon(column: TableColumn<T>): string {
    if (!column.sortable) return '';
    const current = this.sort();
    if (current?.column !== column.key) return 'heroChevronUpDown';
    return current.direction === 'asc' ? 'heroChevronUp' : 'heroChevronDown';
  }

  gotoPage(target: number): void {
    if (target < 1 || target > this.totalPages() || target === this.page.pageNumber) return;
    this.pageChange.emit(target);
  }

  visiblePages(): number[] {
    const total = this.totalPages();
    const current = this.page.pageNumber;
    const window = 2;
    const min = Math.max(1, current - window);
    const max = Math.min(total, current + window);
    const pages: number[] = [];
    for (let i = min; i <= max; i++) pages.push(i);
    return pages;
  }

  pageRangeStart(): number {
    if (this.page.totalCount === 0) return 0;
    return (this.page.pageNumber - 1) * this.page.pageSize + 1;
  }

  pageRangeEnd(): number {
    return Math.min(this.page.pageNumber * this.page.pageSize, this.page.totalCount);
  }
}
