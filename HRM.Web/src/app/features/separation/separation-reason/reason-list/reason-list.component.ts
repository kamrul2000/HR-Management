import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import {
  Component,
  OnInit,
  TemplateRef,
  ViewChild,
  computed,
  inject,
  signal,
} from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { heroPlus, heroPencilSquare, heroTrash } from '@ng-icons/heroicons/outline';

import { ConfirmService } from '../../../../core/services/confirm.service';
import { ToastService } from '../../../../core/services/toast.service';
import { DataTableComponent } from '../../../../shared/components/data-table/data-table.component';
import { TableColumn } from '../../../../shared/components/data-table/data-table.types';
import { DrawerComponent } from '../../../../shared/components/drawer/drawer.component';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';
import {
  CreateSeparationReasonDto,
  SeparationReasonResponse,
  SeparationType,
  UpdateSeparationReasonDto,
} from '../../models/separation-reason.model';
import { SeparationReasonService } from '../../services/separation-reason.service';

const SEPARATION_TYPES: SeparationType[] = ['Resignation', 'Termination', 'Retirement', 'Death', 'Contract End'];

@Component({
  selector: 'hrm-separation-reason-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    NgIcon,
    PageHeaderComponent,
    DataTableComponent,
    StatusBadgeComponent,
    DrawerComponent,
  ],
  providers: [provideIcons({ heroPlus, heroPencilSquare, heroTrash })],
  templateUrl: './reason-list.component.html',
})
export class ReasonListComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(SeparationReasonService);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmService);

  readonly rows = signal<SeparationReasonResponse[]>([]);
  readonly loading = signal(true);
  readonly drawerOpen = signal(false);
  readonly editing = signal<SeparationReasonResponse | null>(null);
  readonly saving = signal(false);

  readonly types = SEPARATION_TYPES;

  readonly form = this.fb.nonNullable.group({
    reasonName: ['', [Validators.required, Validators.maxLength(150)]],
    separationType: ['Resignation' as SeparationType, [Validators.required]],
    description: [''],
    displayOrder: [10, [Validators.required, Validators.min(1), Validators.max(999)]],
    isActive: [true],
  });

  readonly isEdit = computed(() => !!this.editing());

  @ViewChild('typeCellTpl',   { static: true }) typeCellTpl!:   TemplateRef<{ $implicit: SeparationReasonResponse }>;
  @ViewChild('activeTpl',     { static: true }) activeTpl!:     TemplateRef<{ $implicit: SeparationReasonResponse }>;
  @ViewChild('actionsTpl',    { static: true }) actionsTpl!:    TemplateRef<{ $implicit: SeparationReasonResponse }>;

  columns: TableColumn<SeparationReasonResponse>[] = [];

  ngOnInit(): void {
    this.columns = [
      { key: 'displayOrder',   label: '#', width: '60px', align: 'center' },
      { key: 'reasonName',     label: 'Reason' },
      { key: 'separationType', label: 'Type', template: this.typeCellTpl, width: '160px' },
      { key: 'description',    label: 'Description' },
      { key: 'isActive',       label: 'Status', template: this.activeTpl, align: 'center', width: '110px' },
    ];
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.service.getAll().subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) this.rows.set(res.data);
      },
      error: () => this.loading.set(false),
    });
  }

  openCreate(): void {
    this.editing.set(null);
    this.form.reset({
      reasonName: '',
      separationType: 'Resignation',
      description: '',
      displayOrder: (this.rows().length + 1) * 10,
      isActive: true,
    });
    this.drawerOpen.set(true);
  }

  openEdit(row: SeparationReasonResponse): void {
    this.editing.set(row);
    this.form.patchValue({
      reasonName: row.reasonName,
      separationType: row.separationType,
      description: row.description ?? '',
      displayOrder: row.displayOrder,
      isActive: row.isActive,
    });
    this.drawerOpen.set(true);
  }

  closeDrawer(): void {
    this.drawerOpen.set(false);
    this.editing.set(null);
  }

  submit(): void {
    if (this.saving()) return;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const raw = this.form.getRawValue();
    this.saving.set(true);

    if (this.editing()) {
      const dto: UpdateSeparationReasonDto = {
        reasonName: raw.reasonName.trim(),
        separationType: raw.separationType,
        description: raw.description?.trim() || null,
        displayOrder: +raw.displayOrder,
        isActive: raw.isActive,
      };
      this.service.update(this.editing()!.id, dto).subscribe({
        next: (res) => this.afterSave(res, 'updated'),
        error: (err: HttpErrorResponse) => this.afterError(err),
      });
    } else {
      const dto: CreateSeparationReasonDto = {
        reasonName: raw.reasonName.trim(),
        separationType: raw.separationType,
        description: raw.description?.trim() || null,
        displayOrder: +raw.displayOrder,
      };
      this.service.create(dto).subscribe({
        next: (res) => this.afterSave(res, 'created'),
        error: (err: HttpErrorResponse) => this.afterError(err),
      });
    }
  }

  delete(row: SeparationReasonResponse): void {
    this.confirm.confirm({
      title: 'Delete reason',
      message: `Delete separation reason "${row.reasonName}"?`,
      confirmLabel: 'Delete',
      danger: true,
    }).subscribe((ok) => {
      if (!ok) return;
      this.service.delete(row.id).subscribe({
        next: (res) => {
          if (res.success) {
            this.toast.success('Reason deleted.');
            this.rows.set(this.rows().filter((r) => r.id !== row.id));
          } else {
            this.toast.error(res.message || 'Delete failed.');
          }
        },
        error: (err) => this.toast.error(err.error?.message || 'Delete failed.'),
      });
    });
  }

  private afterSave(res: { success: boolean; message: string }, what: string): void {
    this.saving.set(false);
    if (res.success) {
      this.toast.success(`Reason ${what}.`);
      this.closeDrawer();
      this.load();
    } else {
      this.toast.error(res.message || 'Save failed.');
    }
  }

  private afterError(err: HttpErrorResponse): void {
    this.saving.set(false);
    this.toast.error(err.error?.message || 'Save failed.');
  }
}
