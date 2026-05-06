import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { NgIcon, provideIcons } from '@ng-icons/core';
import {
  heroArrowLeft,
  heroPencilSquare,
  heroCamera,
  heroXMark,
} from '@ng-icons/heroicons/outline';

import { ToastService } from '../../../core/services/toast.service';
import { AvatarComponent } from '../../../shared/components/avatar/avatar.component';
import { LoadingSkeletonComponent } from '../../../shared/components/loading-skeleton/loading-skeleton.component';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { EmployeeResponse } from '../models/employee.model';
import { PhotoUploadComponent } from '../photo-upload/photo-upload.component';
import { EmployeeService } from '../services/employee.service';
import { EducationTabComponent } from './tabs/education/education-tab.component';
import { EmergencyContactsTabComponent } from './tabs/emergency-contacts/emergency-contacts-tab.component';
import { ExperienceTabComponent } from './tabs/experience/experience-tab.component';
import { PersonalInfoTabComponent } from './tabs/personal-info/personal-info-tab.component';

type TabKey = 'personal' | 'contacts' | 'education' | 'experience';

interface TabDef {
  key: TabKey;
  label: string;
}

@Component({
  selector: 'hrm-employee-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    NgIcon,
    PageHeaderComponent,
    LoadingSkeletonComponent,
    AvatarComponent,
    StatusBadgeComponent,
    PhotoUploadComponent,
    PersonalInfoTabComponent,
    EmergencyContactsTabComponent,
    EducationTabComponent,
    ExperienceTabComponent,
  ],
  providers: [
    provideIcons({
      heroArrowLeft,
      heroPencilSquare,
      heroCamera,
      heroXMark,
    }),
  ],
  templateUrl: './employee-detail.component.html',
  styleUrl: './employee-detail.component.scss',
})
export class EmployeeDetailComponent implements OnInit {
  private readonly service = inject(EmployeeService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  readonly employee = signal<EmployeeResponse | null>(null);
  readonly loading = signal(true);
  readonly photoOpen = signal(false);
  readonly activeTab = signal<TabKey>('personal');

  readonly tabs: TabDef[] = [
    { key: 'personal',   label: 'Personal Info' },
    { key: 'contacts',   label: 'Emergency Contacts' },
    { key: 'education',  label: 'Education' },
    { key: 'experience', label: 'Experience' },
  ];

  readonly id = computed(() => Number(this.route.snapshot.paramMap.get('id')));

  ngOnInit(): void {
    this.load(this.id());
  }

  load(id: number): void {
    if (!id || Number.isNaN(id)) {
      this.toast.error('Invalid employee ID.');
      this.router.navigate(['/employees']);
      return;
    }
    this.loading.set(true);
    this.service.getById(id).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.employee.set(res.data);
        } else {
          this.toast.error(res.message || 'Could not load employee.');
          this.router.navigate(['/employees']);
        }
      },
      error: () => {
        this.loading.set(false);
        this.router.navigate(['/employees']);
      },
    });
  }

  selectTab(key: TabKey): void { this.activeTab.set(key); }

  togglePhoto(): void { this.photoOpen.update((v) => !v); }

  onPhotoUploaded(updated: EmployeeResponse): void {
    this.employee.set(updated);
    this.photoOpen.set(false);
  }
}
