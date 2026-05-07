import { CommonModule } from '@angular/common';
import {
  Component,
  OnInit,
  TemplateRef,
  ViewChild,
  computed,
  inject,
  signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgIcon, provideIcons } from '@ng-icons/core';
import {
  heroPlus,
  heroPencilSquare,
  heroTrash,
  heroMagnifyingGlass,
  heroCheck,
  heroXMark,
} from '@ng-icons/heroicons/outline';

import { ConfirmService } from '../../../../core/services/confirm.service';
import { ToastService } from '../../../../core/services/toast.service';
import { DataTableComponent } from '../../../../shared/components/data-table/data-table.component';
import { TableColumn } from '../../../../shared/components/data-table/data-table.types';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';
import { LeaveTypeResponse } from '../../models/leave-type.model';
import { LeaveTypeService } from '../../services/leave-type.service';
import { LeaveTypeFormComponent } from '../leave-type-form/leave-type-form.component';

type DrawerMode = 'closed' | 'create' | { mode: 'edit'; leaveType: LeaveTypeResponse };

@Component({
  selector: 'hrm-leave-type-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    NgIcon,
    PageHeaderComponent,
    DataTableComponent,
    StatusBadgeComponent,
    LeaveTypeFormComponent,
  ],
  providers: [
    provideIcons({
      heroPlus,
      heroPencilSquare,
      heroTrash,
      heroMagnifyingGlass,
      heroCheck,
      heroXMark,
    }),
  ],
  templateUrl: './leave-type-list.component.html',
  styleUrl: './leave-type-list.component.scss',
})
export class LeaveTypeListComponent implements OnInit {
  private readonly service = inject(LeaveTypeService);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmService);

  readonly all = signal<LeaveTypeResponse[]>([]);
  readonly loading = signal(true);
  readonly searchTerm = signal('');
  readonly drawer = signal<DrawerMode>('closed');

  @ViewChild('paidCellTpl', { static: true })       paidCellTpl!:       TemplateRef<{ $implicit: LeaveTypeResponse }>;
  @ViewChild('carryCellTpl', { static: true })      carryCellTpl!:      TemplateRef<{ $implicit: LeaveTypeResponse }>;
  @ViewChild('approvalCellTpl', { static: true })   approvalCellTpl!:   TemplateRef<{ $implicit: LeaveTypeResponse }>;
  @ViewChild('docCellTpl', { static: true })        docCellTpl!:        TemplateRef<{ $implicit: LeaveTypeResponse }>;
  @ViewChild('statusCellTpl', { static: true })     statusCellTpl!:     TemplateRef<{ $implicit: LeaveTypeResponse }>;

  columns: TableColumn<LeaveTypeResponse>[] = [];

  readonly filtered = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();
    if (!term) return this.all();
    return this.all().filter((t) =>
      [t.name, t.code, t.description ?? ''].some((v) => v.toLowerCase().includes(term)),
    );
  });

  readonly drawerOpen = computed(() => this.drawer() !== 'closed');
  readonly editing = computed<LeaveTypeResponse | null>(() => {
    const d = this.drawer();
    return typeof d === 'object' ? d.leaveType : null;
  });

  ngOnInit(): void {
    this.columns = [
      { key: 'name',             label: 'Name', sortable: true },
      { key: 'code',             label: 'Code', width: '90px' },
      { key: 'isPaid',           label: 'Paid',     template: this.paidCellTpl,     align: 'center', width: '90px' },
      { key: 'isCarryForward',   label: 'Carry FW', template: this.carryCellTpl,    align: 'center', width: '120px' },
      { key: 'requiresApproval', label: 'Approval', template: this.approvalCellTpl, align: 'center', width: '110px' },
      { key: 'requiresDocument', label: 'Document', template: this.docCellTpl,      align: 'center', width: '110px' },
      { key: 'isActive',         label: 'Status',   template: this.statusCellTpl,   align: 'center', width: '110px' },
    ];
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.service.getAll({ pageSize: 100 }).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) this.all.set(res.data.items);
      },
      error: () => this.loading.set(false),
    });
  }

  openCreate(): void { this.drawer.set('create'); }
  openEdit(leaveType: LeaveTypeResponse): void { this.drawer.set({ mode: 'edit', leaveType }); }
  closeDrawer(): void { this.drawer.set('closed'); }

  onSaved(updated: LeaveTypeResponse): void {
    const exists = this.all().some((t) => t.id === updated.id);
    if (exists) {
      this.all.set(this.all().map((t) => (t.id === updated.id ? updated : t)));
    } else {
      this.all.set([updated, ...this.all()]);
    }
    this.closeDrawer();
  }

  delete(leaveType: LeaveTypeResponse): void {
    this.confirm
      .confirm({
        title: 'Delete leave type',
        message: `Delete "${leaveType.name}"? This may affect existing allotments and applications.`,
        confirmLabel: 'Delete',
        danger: true,
      })
      .subscribe((ok) => {
        if (!ok) return;
        this.service.delete(leaveType.id).subscribe({
          next: (res) => {
            if (res.success) {
              this.toast.success('Leave type deleted.');
              this.all.set(this.all().filter((t) => t.id !== leaveType.id));
            } else {
              this.toast.error(res.message || 'Failed to delete leave type.');
            }
          },
          error: (err) => this.toast.error(err.error?.message || 'Failed to delete leave type.'),
        });
      });
  }
}
