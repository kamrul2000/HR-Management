import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { StatCardComponent } from '../../shared/components/stat-card/stat-card.component';

/**
 * Placeholder dashboard — replaced by the full implementation in Group A.
 * Verifies that shell + shared components render correctly.
 */
@Component({
  selector: 'hrm-dashboard-placeholder',
  standalone: true,
  imports: [CommonModule, PageHeaderComponent, StatCardComponent],
  template: `
    <hrm-page-header
      title="Dashboard"
      subtitle="Foundation scaffolding — Group A will populate the real dashboard."
    >
    </hrm-page-header>

    <div class="page-content">
      <div class="stat-grid">
        <hrm-stat-card label="Total Employees" value="—" icon="heroUsers" tone="primary"></hrm-stat-card>
        <hrm-stat-card label="Present Today"   value="—" icon="heroBriefcase" tone="success"></hrm-stat-card>
        <hrm-stat-card label="Pending Leaves"  value="—" icon="heroDocumentText" tone="warning"></hrm-stat-card>
        <hrm-stat-card label="Pending Loans"   value="—" icon="heroBanknotes" tone="info"></hrm-stat-card>
      </div>

      <div class="card mt-lg">
        <div class="card__header"><h3>Group 0 Foundation Ready</h3></div>
        <div class="card__body">
          <p>
            Shell, sidebar, topbar, breadcrumb, toast service, confirm service, data-table,
            stat-card, status-badge, empty-state, loading-skeleton, file-upload, page-header,
            and the SCSS design system are all wired. Subsequent feature groups (A–K) build on
            this baseline.
          </p>
        </div>
      </div>
    </div>
  `,
  styles: [
    `
      .stat-grid {
        display: grid;
        gap: 16px;
        grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
      }
    `,
  ],
})
export class DashboardComponent {}
