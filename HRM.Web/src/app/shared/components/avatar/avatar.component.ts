import { CommonModule } from '@angular/common';
import { Component, Input, computed } from '@angular/core';

const PALETTE = [
  { bg: '#EFF6FF', fg: '#2563EB' },
  { bg: '#F0FDF4', fg: '#16A34A' },
  { bg: '#FFFBEB', fg: '#D97706' },
  { bg: '#FEF2F2', fg: '#DC2626' },
  { bg: '#F0F9FF', fg: '#0284C7' },
  { bg: '#F5F3FF', fg: '#7C3AED' },
  { bg: '#FFF7ED', fg: '#C2410C' },
  { bg: '#ECFEFF', fg: '#0891B2' },
];

/**
 * Round avatar with photo fallback to color-coded initials.
 * Color is derived from a hash of `name` so the same person always gets the
 * same colour across the app.
 */
@Component({
  selector: 'hrm-avatar',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div
      class="avatar"
      [style.width.px]="size"
      [style.height.px]="size"
      [style.fontSize.px]="fontSize()"
      [style.background]="palette().bg"
      [style.color]="palette().fg"
      [attr.aria-label]="name"
    >
      <img *ngIf="photoUrl; else initialsTpl" [src]="photoUrl" [alt]="name" />
      <ng-template #initialsTpl>{{ initials() }}</ng-template>
    </div>
  `,
  styles: [
    `
      .avatar {
        border-radius: 50%;
        overflow: hidden;
        display: inline-flex;
        align-items: center;
        justify-content: center;
        font-weight: 600;
        flex-shrink: 0;
        letter-spacing: 0.4px;

        img {
          width: 100%;
          height: 100%;
          object-fit: cover;
        }
      }
    `,
  ],
})
export class AvatarComponent {
  @Input({ required: true }) name = '';
  @Input() photoUrl?: string | null;
  @Input() size = 36;

  readonly initials = computed(() => {
    const value = (this.name ?? '').trim();
    if (!value) return '?';
    return value
      .split(/\s+/)
      .slice(0, 2)
      .map((p) => p.charAt(0).toUpperCase())
      .join('');
  });

  readonly fontSize = computed(() => Math.round(this.size * 0.42));

  readonly palette = computed(() => {
    const value = this.name ?? '';
    let hash = 0;
    for (let i = 0; i < value.length; i++) {
      hash = (hash * 31 + value.charCodeAt(i)) | 0;
    }
    const idx = Math.abs(hash) % PALETTE.length;
    return PALETTE[idx];
  });
}
