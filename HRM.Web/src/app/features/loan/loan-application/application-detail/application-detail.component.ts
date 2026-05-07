import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { NgIcon, provideIcons } from '@ng-icons/core';
import {
  heroArrowLeft,
  heroCheck,
  heroXMark,
  heroDocumentText,
  heroBanknotes,
  heroPaperAirplane,
} from '@ng-icons/heroicons/outline';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { ToastService } from '../../../../core/services/toast.service';
import { AvatarComponent } from '../../../../shared/components/avatar/avatar.component';
import { LoadingSkeletonComponent } from '../../../../shared/components/loading-skeleton/loading-skeleton.component';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';
import { CurrencyBdPipe } from '../../../../shared/pipes/currency-bd.pipe';
import { LoanApplicationResponse } from '../../models/loan-application.model';
import { LoanApprovalResponse } from '../../models/loan-approval.model';
import { LoanRecommendationResponse } from '../../models/loan-recommendation.model';
import { EmployeeLoanService } from '../../services/employee-loan.service';
import { LoanApplicationService } from '../../services/loan-application.service';
import { LoanApprovalService } from '../../services/loan-approval.service';
import { LoanRecommendationService } from '../../services/loan-recommendation.service';
import { ApprovalDrawerComponent } from '../../shared/approval-drawer/approval-drawer.component';
import { DisburseDrawerComponent } from '../../shared/disburse-drawer/disburse-drawer.component';
import { RecommendationDrawerComponent } from '../../shared/recommendation-drawer/recommendation-drawer.component';

interface TimelineEvent {
  key: string;
  title: string;
  date?: string | null;
  description?: string;
  state: 'done' | 'current' | 'rejected' | 'pending';
}

@Component({
  selector: 'hrm-loan-application-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    NgIcon,
    PageHeaderComponent,
    LoadingSkeletonComponent,
    AvatarComponent,
    StatusBadgeComponent,
    CurrencyBdPipe,
    RecommendationDrawerComponent,
    ApprovalDrawerComponent,
    DisburseDrawerComponent,
  ],
  providers: [provideIcons({ heroArrowLeft, heroCheck, heroXMark, heroDocumentText, heroBanknotes, heroPaperAirplane })],
  templateUrl: './application-detail.component.html',
  styleUrl: './application-detail.component.scss',
})
export class ApplicationDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  private readonly applications = inject(LoanApplicationService);
  private readonly recommendations = inject(LoanRecommendationService);
  private readonly approvals = inject(LoanApprovalService);
  private readonly loans = inject(EmployeeLoanService);

  readonly application = signal<LoanApplicationResponse | null>(null);
  readonly recommendation = signal<LoanRecommendationResponse | null>(null);
  readonly approval = signal<LoanApprovalResponse | null>(null);
  readonly disbursedLoanId = signal<number | null>(null);
  readonly loading = signal(true);

  readonly recommendOpen = signal(false);
  readonly approveOpen = signal(false);
  readonly disburseOpen = signal(false);

  readonly canRecommend = computed(() => this.application()?.status === 'Pending');
  readonly canApprove = computed(() => this.application()?.status === 'Recommended');
  readonly canDisburse = computed(() => this.application()?.status === 'Approved');

  readonly timeline = computed<TimelineEvent[]>(() => {
    const app = this.application();
    if (!app) return [];
    const rec = this.recommendation();
    const apr = this.approval();
    const events: TimelineEvent[] = [];

    events.push({
      key: 'submitted',
      title: 'Application submitted',
      date: app.createdAt,
      description: `${app.employeeFullName} requested ${app.requestedAmountFormatted || app.requestedAmount} for ${app.requestedTenureMonths} months.`,
      state: 'done',
    });

    if (rec) {
      events.push({
        key: 'recommended',
        title: rec.decision === 'Reject' ? 'Recommendation: Reject' : 'Recommendation: Approve',
        date: rec.createdAt,
        description: rec.decision === 'Reject'
          ? rec.remarks
          : `Recommended ${rec.recommendedAmountFormatted || rec.recommendedAmount} for ${rec.recommendedTenureMonths} months. ${rec.remarks}`,
        state: rec.decision === 'Reject' ? 'rejected' : 'done',
      });
    } else if (app.status === 'Pending') {
      events.push({
        key: 'awaiting-recommendation',
        title: 'Awaiting recommendation',
        state: 'current',
      });
    }

    if (apr) {
      events.push({
        key: 'approved',
        title: apr.decision === 'Reject' ? 'Approval: Rejected' : 'Approval: Approved',
        date: apr.createdAt,
        description: apr.decision === 'Reject'
          ? apr.remarks
          : `Approved ${apr.approvedAmountFormatted || apr.approvedAmount} @ ${apr.interestRate}% (${apr.interestTypeLabel || apr.interestType}). EMI ${apr.monthlyInstallmentFormatted || apr.monthlyInstallment}.`,
        state: apr.decision === 'Reject' ? 'rejected' : 'done',
      });
    } else if (app.status === 'Recommended') {
      events.push({
        key: 'awaiting-approval',
        title: 'Awaiting approval',
        state: 'current',
      });
    }

    if (app.status === 'Disbursed') {
      events.push({
        key: 'disbursed',
        title: 'Loan disbursed',
        description: 'Repayment schedule generated.',
        state: 'done',
      });
    } else if (app.status === 'Approved') {
      events.push({
        key: 'awaiting-disbursement',
        title: 'Awaiting disbursement',
        state: 'current',
      });
    }

    if (app.status === 'Cancelled') {
      events.push({
        key: 'cancelled',
        title: 'Application cancelled',
        date: app.updatedAt,
        state: 'rejected',
      });
    }

    return events;
  });

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!id) {
      this.toast.error('Invalid application id.');
      this.router.navigate(['/loans/applications']);
      return;
    }
    this.loadAll(id);
  }

  private loadAll(id: number): void {
    this.loading.set(true);
    forkJoin({
      app: this.applications.getById(id),
      rec: this.recommendations.getByApplication(id).pipe(catchError(() => of(null))),
      apr: this.approvals.getByApplication(id).pipe(catchError(() => of(null))),
    }).subscribe({
      next: ({ app, rec, apr }) => {
        this.loading.set(false);
        if (!app.success || !app.data) {
          this.router.navigate(['/loans/applications']);
          return;
        }
        this.application.set(app.data);
        this.recommendation.set(rec && (rec as { success: boolean }).success ? (rec as { data: LoanRecommendationResponse }).data : null);
        this.approval.set(apr && (apr as { success: boolean }).success ? (apr as { data: LoanApprovalResponse }).data : null);
      },
      error: () => {
        this.loading.set(false);
        this.router.navigate(['/loans/applications']);
      },
    });
  }

  openRecommend(): void { this.recommendOpen.set(true); }
  closeRecommend(): void { this.recommendOpen.set(false); }
  onRecommended(): void {
    this.closeRecommend();
    const id = this.application()?.id;
    if (id) this.loadAll(id);
  }

  openApprove(): void { this.approveOpen.set(true); }
  closeApprove(): void { this.approveOpen.set(false); }
  onApproved(): void {
    this.closeApprove();
    const id = this.application()?.id;
    if (id) this.loadAll(id);
  }

  openDisburse(): void { this.disburseOpen.set(true); }
  closeDisburse(): void { this.disburseOpen.set(false); }
  onDisbursed(loanId: number): void {
    this.closeDisburse();
    this.disbursedLoanId.set(loanId);
    const id = this.application()?.id;
    if (id) this.loadAll(id);
  }

  goToLoan(): void {
    const loanId = this.disbursedLoanId();
    if (loanId) this.router.navigate(['/loans/active', loanId]);
  }
}
