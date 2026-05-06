import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { NgIcon, provideIcons } from '@ng-icons/core';
import {
  heroPlus,
  heroPencilSquare,
  heroTrash,
  heroClock,
  heroMoon,
} from '@ng-icons/heroicons/outline';

import { ConfirmService } from '../../../../core/services/confirm.service';
import { ToastService } from '../../../../core/services/toast.service';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';
import { LoadingSkeletonComponent } from '../../../../shared/components/loading-skeleton/loading-skeleton.component';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import { DutySlotResponse } from '../../models/duty-slot.model';
import { DutySlotService } from '../../services/duty-slot.service';
import { DutySlotFormComponent } from '../duty-slot-form/duty-slot-form.component';

type DrawerMode = 'closed' | 'create' | { mode: 'edit'; slot: DutySlotResponse };

@Component({
  selector: 'hrm-duty-slot-list',
  standalone: true,
  imports: [
    CommonModule,
    NgIcon,
    PageHeaderComponent,
    LoadingSkeletonComponent,
    EmptyStateComponent,
    DutySlotFormComponent,
  ],
  providers: [
    provideIcons({ heroPlus, heroPencilSquare, heroTrash, heroClock, heroMoon }),
  ],
  templateUrl: './duty-slot-list.component.html',
  styleUrl: './duty-slot-list.component.scss',
})
export class DutySlotListComponent implements OnInit {
  private readonly service = inject(DutySlotService);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmService);

  readonly slots = signal<DutySlotResponse[]>([]);
  readonly loading = signal(true);
  readonly drawer = signal<DrawerMode>('closed');

  readonly drawerOpen = computed(() => this.drawer() !== 'closed');
  readonly editingSlot = computed<DutySlotResponse | null>(() => {
    const d = this.drawer();
    return typeof d === 'object' ? d.slot : null;
  });

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.service.getAll({ pageSize: 100 }).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) this.slots.set(res.data.items);
      },
      error: () => this.loading.set(false),
    });
  }

  openCreate(): void { this.drawer.set('create'); }
  openEdit(slot: DutySlotResponse): void { this.drawer.set({ mode: 'edit', slot }); }
  closeDrawer(): void { this.drawer.set('closed'); }

  onSaved(updated: DutySlotResponse): void {
    const exists = this.slots().some((s) => s.id === updated.id);
    if (exists) {
      this.slots.set(this.slots().map((s) => (s.id === updated.id ? updated : s)));
    } else {
      this.slots.set([updated, ...this.slots()]);
    }
    this.closeDrawer();
  }

  delete(slot: DutySlotResponse): void {
    this.confirm
      .confirm({
        title: 'Delete duty slot',
        message: `Delete "${slot.slotName}"? This may affect attendance entries that reference it.`,
        confirmLabel: 'Delete',
        danger: true,
      })
      .subscribe((ok) => {
        if (!ok) return;
        this.service.delete(slot.id).subscribe({
          next: (res) => {
            if (res.success) {
              this.toast.success('Duty slot deleted.');
              this.slots.set(this.slots().filter((s) => s.id !== slot.id));
            } else {
              this.toast.error(res.message || 'Failed to delete slot.');
            }
          },
          error: (err) => this.toast.error(err.error?.message || 'Failed to delete slot.'),
        });
      });
  }

  formatTime(value: string): string {
    if (!value) return '—';
    return value.length >= 5 ? value.slice(0, 5) : value;
  }

  hoursLabel(value: number): string {
    const h = Math.floor(value);
    const m = Math.round((value - h) * 60);
    return m > 0 ? `${h}h ${m}m` : `${h}h`;
  }
}
