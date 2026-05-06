import { TemplateRef } from '@angular/core';

/**
 * Cell template context. Both `$implicit` and the named `row` slot point at
 * the same object, but `row` is marked optional so callers can declare a
 * narrower context type (e.g. `TemplateRef<{ $implicit: Foo }>`) without TS
 * complaining about a missing `row` member.
 */
export interface CellContext<T> {
  $implicit: T;
  row?: T;
}

export interface TableColumn<T = unknown> {
  key: string;
  label: string;
  sortable?: boolean;
  width?: string;
  align?: 'left' | 'center' | 'right';
  template?: TemplateRef<CellContext<T>>;
}

export interface SortState {
  column: string;
  direction: 'asc' | 'desc';
}

export interface PageState {
  pageNumber: number;
  pageSize: number;
  totalCount: number;
}
