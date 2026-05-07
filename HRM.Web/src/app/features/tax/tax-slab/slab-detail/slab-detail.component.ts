import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { heroArrowLeft, heroPencilSquare, heroCalculator } from '@ng-icons/heroicons/outline';

import { LoadingSkeletonComponent } from '../../../../shared/components/loading-skeleton/loading-skeleton.component';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';
import { CurrencyBdPipe } from '../../../../shared/pipes/currency-bd.pipe';
import { TaxSlabConfigResponse } from '../../models/tax-slab.model';
import { TaxSlabService } from '../../services/tax-slab.service';
import { TaxComputeDrawerComponent } from '../tax-compute-drawer/tax-compute-drawer.component';

@Component({
  selector: 'hrm-tax-slab-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    NgIcon,
    PageHeaderComponent,
    LoadingSkeletonComponent,
    StatusBadgeComponent,
    CurrencyBdPipe,
    TaxComputeDrawerComponent,
  ],
  providers: [provideIcons({ heroArrowLeft, heroPencilSquare, heroCalculator })],
  templateUrl: './slab-detail.component.html',
  styleUrls: ['../slab-form/slab-form.component.scss'],
})
export class SlabDetailComponent implements OnInit {
  private readonly service = inject(TaxSlabService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly config = signal<TaxSlabConfigResponse | null>(null);
  readonly loading = signal(true);
  readonly computeOpen = signal(false);

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!id) {
      this.router.navigate(['/tax/slabs']);
      return;
    }
    this.service.getById(id).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) this.config.set(res.data);
        else this.router.navigate(['/tax/slabs']);
      },
      error: () => {
        this.loading.set(false);
        this.router.navigate(['/tax/slabs']);
      },
    });
  }

  openCompute(): void { this.computeOpen.set(true); }
  closeCompute(): void { this.computeOpen.set(false); }
}
