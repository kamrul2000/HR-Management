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
import { Router, RouterLink } from '@angular/router';
import { NgIcon, provideIcons } from '@ng-icons/core';
import {
  heroPlus,
  heroEye,
  heroPencilSquare,
  heroMagnifyingGlass,
} from '@ng-icons/heroicons/outline';

import { ToastService } from '../../../../core/services/toast.service';
import { AvatarComponent } from '../../../../shared/components/avatar/avatar.component';
import { DataTableComponent } from '../../../../shared/components/data-table/data-table.component';
import { TableColumn } from '../../../../shared/components/data-table/data-table.types';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import { CurrencyBdPipe } from '../../../../shared/pipes/currency-bd.pipe';
import { SalaryStructureResponse } from '../../models/salary-structure.model';
import { SalaryStructureService } from '../../services/salary-structure.service';

@Component({
  selector: 'hrm-structure-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    NgIcon,
    PageHeaderComponent,
    DataTableComponent,
    AvatarComponent,
    CurrencyBdPipe,
  ],
  providers: [provideIcons({ heroPlus, heroEye, heroPencilSquare, heroMagnifyingGlass })],
  templateUrl: './structure-list.component.html',
  styleUrl: './structure-list.component.scss',
})
export class StructureListComponent implements OnInit {
  private readonly service = inject(SalaryStructureService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);

  readonly all = signal<SalaryStructureResponse[]>([]);
  readonly loading = signal(true);
  readonly searchTerm = signal('');

  @ViewChild('employeeCellTpl', { static: true }) employeeCellTpl!: TemplateRef<{ $implicit: SalaryStructureResponse }>;
  @ViewChild('basicCellTpl', { static: true })    basicCellTpl!:    TemplateRef<{ $implicit: SalaryStructureResponse }>;
  @ViewChild('grossCellTpl', { static: true })    grossCellTpl!:    TemplateRef<{ $implicit: SalaryStructureResponse }>;
  @ViewChild('netCellTpl', { static: true })      netCellTpl!:      TemplateRef<{ $implicit: SalaryStructureResponse }>;

  columns: TableColumn<SalaryStructureResponse>[] = [];

  readonly filtered = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();
    if (!term) return this.all();
    return this.all().filter((s) =>
      [s.employeeFullName ?? '', s.employeeCode ?? ''].some((v) =>
        v.toLowerCase().includes(term),
      ),
    );
  });

  ngOnInit(): void {
    this.columns = [
      { key: 'employee',          label: 'Employee', template: this.employeeCellTpl },
      { key: 'effectiveFrom',     label: 'Effective From', width: '140px' },
      { key: 'basicSalary',       label: 'Basic',     template: this.basicCellTpl, align: 'right', width: '130px' },
      { key: 'estimatedGrossSalary', label: 'Est. Gross', template: this.grossCellTpl, align: 'right', width: '140px' },
      { key: 'estimatedNetSalary',   label: 'Est. Net',   template: this.netCellTpl,   align: 'right', width: '140px' },
    ];
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.service.getAll({ isActive: true, pageSize: 200 }).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) this.all.set(res.data.items);
      },
      error: () => this.loading.set(false),
    });
  }

  view(row: SalaryStructureResponse): void {
    this.router.navigate(['/salary/structures', row.id]);
  }

  edit(row: SalaryStructureResponse): void {
    this.router.navigate(['/salary/structures', row.id, 'edit']);
  }

  formatDate(value: string): string {
    return value?.length >= 10 ? value.slice(0, 10) : value || '—';
  }
}
