import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { heroPlus, heroPencilSquare, heroTrash, heroCalculator } from '@ng-icons/heroicons/outline';

import { ConfirmService } from '../../../../core/services/confirm.service';
import { ToastService } from '../../../../core/services/toast.service';
import { LoadingSkeletonComponent } from '../../../../shared/components/loading-skeleton/loading-skeleton.component';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';
import { CurrencyBdPipe } from '../../../../shared/pipes/currency-bd.pipe';
import { GratuityRuleResponse } from '../../models/gratuity-rule.model';
import { GratuityRuleService } from '../../services/gratuity-rule.service';
import { RuleFormComponent } from '../rule-form/rule-form.component';
import { GratuityPreviewDrawerComponent } from '../gratuity-preview-drawer/gratuity-preview-drawer.component';

@Component({
  selector: 'hrm-gratuity-rule-list',
  standalone: true,
  imports: [
    CommonModule,
    NgIcon,
    PageHeaderComponent,
    LoadingSkeletonComponent,
    StatusBadgeComponent,
    CurrencyBdPipe,
    RuleFormComponent,
    GratuityPreviewDrawerComponent,
  ],
  providers: [provideIcons({ heroPlus, heroPencilSquare, heroTrash, heroCalculator })],
  templateUrl: './rule-list.component.html',
  styleUrl: './rule-list.component.scss',
})
export class RuleListComponent implements OnInit {
  private readonly service = inject(GratuityRuleService);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmService);

  readonly rules = signal<GratuityRuleResponse[]>([]);
  readonly loading = signal(true);

  readonly drawerOpen = signal(false);
  readonly editing = signal<GratuityRuleResponse | null>(null);
  readonly previewOpen = signal(false);

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.service.getAll().subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) this.rules.set(res.data);
      },
      error: () => this.loading.set(false),
    });
  }

  openCreate(): void {
    this.editing.set(null);
    this.drawerOpen.set(true);
  }

  openEdit(rule: GratuityRuleResponse): void {
    this.editing.set(rule);
    this.drawerOpen.set(true);
  }

  closeDrawer(): void {
    this.drawerOpen.set(false);
    this.editing.set(null);
  }

  onSaved(): void {
    this.closeDrawer();
    this.load();
  }

  openPreview(): void { this.previewOpen.set(true); }
  closePreview(): void { this.previewOpen.set(false); }

  delete(rule: GratuityRuleResponse): void {
    this.confirm.confirm({
      title: 'Delete gratuity rule',
      message: `Delete rule "${rule.ruleName}"?`,
      confirmLabel: 'Delete',
      danger: true,
    }).subscribe((ok) => {
      if (!ok) return;
      this.service.delete(rule.id).subscribe({
        next: (res) => {
          if (res.success) {
            this.toast.success('Rule deleted.');
            this.rules.set(this.rules().filter((r) => r.id !== rule.id));
          } else {
            this.toast.error(res.message || 'Delete failed.');
          }
        },
        error: (err) => this.toast.error(err.error?.message || 'Delete failed.'),
      });
    });
  }
}
