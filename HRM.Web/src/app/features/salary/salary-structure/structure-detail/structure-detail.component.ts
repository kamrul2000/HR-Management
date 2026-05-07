import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { heroArrowLeft, heroPencilSquare } from '@ng-icons/heroicons/outline';

import { ToastService } from '../../../../core/services/toast.service';
import { AvatarComponent } from '../../../../shared/components/avatar/avatar.component';
import { LoadingSkeletonComponent } from '../../../../shared/components/loading-skeleton/loading-skeleton.component';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import { CurrencyBdPipe } from '../../../../shared/pipes/currency-bd.pipe';
import { SalaryStructureResponse } from '../../models/salary-structure.model';
import { SalaryStructureService } from '../../services/salary-structure.service';

@Component({
  selector: 'hrm-structure-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    NgIcon,
    PageHeaderComponent,
    LoadingSkeletonComponent,
    AvatarComponent,
    CurrencyBdPipe,
  ],
  providers: [provideIcons({ heroArrowLeft, heroPencilSquare })],
  templateUrl: './structure-detail.component.html',
  styleUrl: './structure-detail.component.scss',
})
export class StructureDetailComponent implements OnInit {
  private readonly service = inject(SalaryStructureService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  readonly structure = signal<SalaryStructureResponse | null>(null);
  readonly loading = signal(true);

  readonly earnings = computed(() =>
    (this.structure()?.items ?? []).filter((i) => i.headType === 'Earning'),
  );
  readonly deductions = computed(() =>
    (this.structure()?.items ?? []).filter((i) => i.headType === 'Deduction'),
  );

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!id) {
      this.toast.error('Invalid structure id.');
      this.router.navigate(['/salary/structures']);
      return;
    }
    this.loading.set(true);
    this.service.getById(id).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) this.structure.set(res.data);
        else this.router.navigate(['/salary/structures']);
      },
      error: () => {
        this.loading.set(false);
        this.router.navigate(['/salary/structures']);
      },
    });
  }
}
