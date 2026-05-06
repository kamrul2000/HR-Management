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
  heroAcademicCap,
  heroPaperClip,
  heroDocumentArrowDown,
} from '@ng-icons/heroicons/outline';

import { ConfirmService } from '../../../../../core/services/confirm.service';
import { ToastService } from '../../../../../core/services/toast.service';
import { EmptyStateComponent } from '../../../../../shared/components/empty-state/empty-state.component';
import { LoadingSkeletonComponent } from '../../../../../shared/components/loading-skeleton/loading-skeleton.component';
import { EducationDto } from '../../../models/additional-info.model';
import { AdditionalInfoService } from '../../../services/additional-info.service';
import { EducationFormComponent } from './education-form.component';

type DrawerMode = 'closed' | 'create' | { mode: 'edit'; record: EducationDto };

@Component({
  selector: 'hrm-education-tab',
  standalone: true,
  imports: [
    CommonModule,
    NgIcon,
    LoadingSkeletonComponent,
    EmptyStateComponent,
    EducationFormComponent,
  ],
  providers: [
    provideIcons({
      heroPlus,
      heroPencilSquare,
      heroTrash,
      heroAcademicCap,
      heroPaperClip,
      heroDocumentArrowDown,
    }),
  ],
  templateUrl: './education-tab.component.html',
  styleUrl: './education-tab.component.scss',
})
export class EducationTabComponent implements OnChanges {
  private readonly service = inject(AdditionalInfoService);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmService);

  @Input({ required: true }) employeeId!: number;

  readonly records = signal<EducationDto[]>([]);
  readonly loading = signal(true);
  readonly drawer = signal<DrawerMode>('closed');

  ngOnChanges(changes: SimpleChanges): void {
    if ('employeeId' in changes && this.employeeId) this.load();
  }

  load(): void {
    this.loading.set(true);
    this.service.getEducation(this.employeeId).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) this.records.set(res.data);
      },
      error: () => this.loading.set(false),
    });
  }

  openCreate(): void { this.drawer.set('create'); }
  openEdit(record: EducationDto): void { this.drawer.set({ mode: 'edit', record }); }
  closeDrawer(): void { this.drawer.set('closed'); }

  onSaved(): void {
    this.closeDrawer();
    this.load();
  }

  delete(record: EducationDto): void {
    this.confirm
      .confirm({
        title: 'Delete education',
        message: `Delete "${record.degree}" from "${record.institution}"? This cannot be undone.`,
        confirmLabel: 'Delete',
        danger: true,
      })
      .subscribe((ok) => {
        if (!ok) return;
        this.service.deleteEducation(record.id).subscribe({
          next: (res) => {
            if (res.success) {
              this.toast.success('Education record removed.');
              this.records.set(this.records().filter((r) => r.id !== record.id));
            } else {
              this.toast.error(res.message || 'Failed to delete education.');
            }
          },
          error: (err: HttpErrorResponse) => {
            this.toast.error(err.error?.message || 'Failed to delete education.');
          },
        });
      });
  }

  onAttachmentChange(record: EducationDto, event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file) return;
    this.service.uploadEducationAttachment(record.id, file).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.toast.success('Certificate uploaded.');
          this.records.set(this.records().map((r) => (r.id === record.id ? res.data! : r)));
        } else {
          this.toast.error(res.message || 'Failed to upload certificate.');
        }
      },
      error: (err: HttpErrorResponse) => {
        this.toast.error(err.error?.message || 'Failed to upload certificate.');
      },
    });
  }

  drawerOpen = () => this.drawer() !== 'closed';
  editingRecord(): EducationDto | null {
    const d = this.drawer();
    return typeof d === 'object' ? d.record : null;
  }
}
