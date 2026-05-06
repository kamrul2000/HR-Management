import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

@Component({
  selector: 'hrm-loading-skeleton',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="skeleton" [attr.aria-busy]="true" aria-label="Loading">
      <div class="skeleton__row" *ngFor="let _ of placeholderArray; let i = index">
        <div class="skeleton__cell skeleton__cell--avatar" *ngIf="showAvatar"></div>
        <div class="skeleton__bars">
          <div class="skeleton__bar skeleton__bar--lg"></div>
          <div class="skeleton__bar skeleton__bar--sm"></div>
        </div>
      </div>
    </div>
  `,
  styleUrl: './loading-skeleton.component.scss',
})
export class LoadingSkeletonComponent {
  @Input() rows: number | string = 5;
  @Input() showAvatar = false;

  get placeholderArray(): unknown[] {
    const count = Math.max(1, Number(this.rows) || 5);
    return Array.from({ length: count });
  }
}
