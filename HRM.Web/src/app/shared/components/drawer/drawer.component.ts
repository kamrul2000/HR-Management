import { CommonModule } from '@angular/common';
import {
  Component,
  EventEmitter,
  HostListener,
  Input,
  Output,
} from '@angular/core';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { heroXMark } from '@ng-icons/heroicons/outline';

export type DrawerSize = 'md' | 'lg';

/**
 * Generic right-side slide-in panel with backdrop.
 *
 * Usage:
 *   <hrm-drawer title="Add Company" (dismiss)="close()">
 *     ...form...
 *     <ng-container drawer-footer>
 *       <button class="btn btn--secondary" (click)="close()">Cancel</button>
 *       <button class="btn btn--primary" (click)="save()">Save</button>
 *     </ng-container>
 *   </hrm-drawer>
 */
@Component({
  selector: 'hrm-drawer',
  standalone: true,
  imports: [CommonModule, NgIcon],
  providers: [provideIcons({ heroXMark })],
  templateUrl: './drawer.component.html',
})
export class DrawerComponent {
  @Input({ required: true }) title!: string;
  @Input() subtitle?: string;
  @Input() size: DrawerSize = 'md';

  /** When true the backdrop click is ignored — useful for in-progress saves. */
  @Input() locked = false;

  @Output() dismiss = new EventEmitter<void>();

  onBackdropClick(): void {
    if (!this.locked) this.dismiss.emit();
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (!this.locked) this.dismiss.emit();
  }
}
