import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { NgIcon, provideIcons } from '@ng-icons/core';
import {
  heroDocumentMagnifyingGlass,
  heroFolderOpen,
} from '@ng-icons/heroicons/outline';

@Component({
  selector: 'hrm-empty-state',
  standalone: true,
  imports: [CommonModule, NgIcon],
  providers: [provideIcons({ heroDocumentMagnifyingGlass, heroFolderOpen })],
  template: `
    <div class="empty-state">
      <ng-icon
        [name]="icon"
        size="56"
        class="empty-state__icon"
        aria-hidden="true"
      />
      <h3 class="empty-state__title">{{ title }}</h3>
      <p class="empty-state__description">{{ description }}</p>
      <button
        *ngIf="actionLabel"
        type="button"
        class="btn btn--primary"
        (click)="action.emit()"
      >
        {{ actionLabel }}
      </button>
    </div>
  `,
  styleUrl: './empty-state.component.scss',
})
export class EmptyStateComponent {
  @Input() icon = 'heroDocumentMagnifyingGlass';
  @Input({ required: true }) title!: string;
  @Input() description = '';
  @Input() actionLabel?: string;

  @Output() action = new EventEmitter<void>();
}
