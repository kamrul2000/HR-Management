import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

export type BadgeVariant =
  | 'active'
  | 'inactive'
  | 'pending'
  | 'approved'
  | 'rejected'
  | 'cancelled'
  | 'draft'
  | 'finalized'
  | 'processed'
  | 'paid'
  | 'overdue'
  | 'skipped'
  | 'present'
  | 'absent'
  | 'late'
  | 'half-day'
  | 'holiday'
  | 'weekly-off'
  | 'recommended'
  | 'disbursed'
  | 'completed'
  | 'eligible'
  | 'ineligible';

@Component({
  selector: 'hrm-status-badge',
  standalone: true,
  imports: [CommonModule],
  template: `<span class="badge" [ngClass]="'badge--' + normalized">{{ label || variant }}</span>`,
  styleUrl: './status-badge.component.scss',
})
export class StatusBadgeComponent {
  @Input({ required: true }) variant!: BadgeVariant | string;
  @Input() label?: string;

  get normalized(): string {
    const value = (this.variant ?? '').toString().toLowerCase().trim();
    return value.replace(/\s+/g, '-');
  }
}
