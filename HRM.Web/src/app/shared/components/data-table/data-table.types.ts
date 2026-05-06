import { TemplateRef } from '@angular/core';

export interface TableColumn<T = unknown> {
  key: string;
  label: string;
  sortable?: boolean;
  width?: string;
  align?: 'left' | 'center' | 'right';
  template?: TemplateRef<{ $implicit: T; row: T }>;
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
