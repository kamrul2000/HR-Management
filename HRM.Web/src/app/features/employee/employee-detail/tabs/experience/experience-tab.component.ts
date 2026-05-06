import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import {
  Component,
  Input,
  OnChanges,
  SimpleChanges,
  inject,
  signal,
} from '@angular/core';
import { NgIcon, provideIcons } from '@ng-icons/core';
import {
  heroPlus,
  heroPencilSquare,
  heroTrash,
  heroBriefcase,
  heroPaperClip,
  heroDocumentArrowDown,
} from '@ng-icons/heroicons/outline';

import { ConfirmService } from '../../../../../core/services/confirm.service';
import { ToastService } from '../../../../../core/services/toast.service';
import { EmptyStateComponent } from '../../../../../shared/components/empty-state/empty-state.component';
import { LoadingSkeletonComponent } from '../../../../../shared/components/loading-skeleton/loading-skeleton.component';
import { ExperienceDto } from '../../../models/additional-info.model';
import { AdditionalInfoService } from '../../../services/additional-info.service';
import { ExperienceFormComponent } from './experience-form.component';

type DrawerMode = 'closed' | 'create' | { mode: 'edit'; record: ExperienceDto };

@Component({
  selector: 'hrm-experience-tab',
  standalone: true,
  imports: [
    CommonModule,
    NgIcon,
    LoadingSkeletonComponent,
    EmptyStateComponent,
    ExperienceFormComponent,
  ],
  providers: [
    provideIcons({
      heroPlus,
      heroPencilSquare,
      heroTrash,
      heroBriefcase,
      heroPaperClip,
      heroDocumentArrowDown,
    }),
  ],
  templateUrl: './experience-tab.component.html',
  styleUrl: './experience-tab.component.scss',
})
export class ExperienceTabComponent implements OnChanges {
  private readonly service = inject(AdditionalInfoService);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmService);

  @Input({ required: true }) employeeId!: number;

  readonly records = signal<ExperienceDto[]>([]);
  readonly loading = signal(true);
  readonly drawer = signal<DrawerMode>('closed');

  ngOnChanges(changes: SimpleChanges): void {
    if ('employeeId' in changes && this.employeeId) this.load();
  }

  load(): void {
    this.loading.set(true);
    this.service.getExperience(this.employeeId).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) this.records.set(res.data);
      },
      error: () => this.loading.set(false),
    });
  }

  openCreate(): void { this.drawer.set('create'); }
  openEdit(record: ExperienceDto): void { this.drawer.set({ mode: 'edit', record }); }
  closeDrawer(): void { this.drawer.set('closed'); }

  onSaved(): void {
    this.closeDrawer();
    this.load();
  }

  delete(record: ExperienceDto): void {
    this.confirm
      .confirm({
        title: 'Delete experience',
        message: `Delete experience at "${record.organizationName}"? This cannot be undone.`,
        confirmLabel: 'Delete',
        danger: true,
      })
      .subscribe((ok) => {
        if (!ok) return;
        this.service.deleteExperience(record.id).subscribe({
          next: (res) => {
            if (res.success) {
              this.toast.success('Experience removed.');
              this.records.set(this.records().filter((r) => r.id !== record.id));
            } else {
              this.toast.error(res.message || 'Failed to delete experience.');
            }
          },
          error: (err: HttpErrorResponse) => {
            this.toast.error(err.error?.message || 'Failed to delete experience.');
          },
        });
      });
  }

  onAttachmentChange(record: ExperienceDto, event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file) return;
    this.service.uploadExperienceAttachment(record.id, file).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.toast.success('Experience letter uploaded.');
          this.records.set(this.records().map((r) => (r.id === record.id ? res.data! : r)));
        } else {
          this.toast.error(res.message || 'Failed to upload letter.');
        }
      },
      error: (err: HttpErrorResponse) => {
        this.toast.error(err.error?.message || 'Failed to upload letter.');
      },
    });
  }

  drawerOpen = () => this.drawer() !== 'closed';
  editingRecord(): ExperienceDto | null {
    const d = this.drawer();
    return typeof d === 'object' ? d.record : null;
  }
}
