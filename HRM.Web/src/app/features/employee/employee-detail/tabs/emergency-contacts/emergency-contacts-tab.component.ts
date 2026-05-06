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
  heroStar,
  heroPencilSquare,
  heroTrash,
  heroPhone,
} from '@ng-icons/heroicons/outline';

import { ConfirmService } from '../../../../../core/services/confirm.service';
import { ToastService } from '../../../../../core/services/toast.service';
import { EmptyStateComponent } from '../../../../../shared/components/empty-state/empty-state.component';
import { LoadingSkeletonComponent } from '../../../../../shared/components/loading-skeleton/loading-skeleton.component';
import { EmergencyContactDto } from '../../../models/additional-info.model';
import { AdditionalInfoService } from '../../../services/additional-info.service';
import { EmergencyContactFormComponent } from './emergency-contact-form.component';

type DrawerMode = 'closed' | 'create' | { mode: 'edit'; contact: EmergencyContactDto };

@Component({
  selector: 'hrm-emergency-contacts-tab',
  standalone: true,
  imports: [
    CommonModule,
    NgIcon,
    LoadingSkeletonComponent,
    EmptyStateComponent,
    EmergencyContactFormComponent,
  ],
  providers: [
    provideIcons({ heroPlus, heroStar, heroPencilSquare, heroTrash, heroPhone }),
  ],
  templateUrl: './emergency-contacts-tab.component.html',
  styleUrl: './emergency-contacts-tab.component.scss',
})
export class EmergencyContactsTabComponent implements OnChanges {
  private readonly service = inject(AdditionalInfoService);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmService);

  @Input({ required: true }) employeeId!: number;

  readonly contacts = signal<EmergencyContactDto[]>([]);
  readonly loading = signal(true);
  readonly drawer = signal<DrawerMode>('closed');

  ngOnChanges(changes: SimpleChanges): void {
    if ('employeeId' in changes && this.employeeId) this.load();
  }

  load(): void {
    this.loading.set(true);
    this.service.getContacts(this.employeeId).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) this.contacts.set(res.data);
      },
      error: () => this.loading.set(false),
    });
  }

  openCreate(): void { this.drawer.set('create'); }
  openEdit(contact: EmergencyContactDto): void { this.drawer.set({ mode: 'edit', contact }); }
  closeDrawer(): void { this.drawer.set('closed'); }

  onSaved(): void {
    this.closeDrawer();
    this.load();
  }

  setPrimary(contact: EmergencyContactDto): void {
    if (contact.isPrimary) return;
    this.service.setPrimaryContact(contact.id).subscribe({
      next: (res) => {
        if (res.success) {
          this.toast.success('Primary contact updated.');
          this.load();
        } else {
          this.toast.error(res.message || 'Failed to set primary contact.');
        }
      },
      error: (err: HttpErrorResponse) => {
        this.toast.error(err.error?.message || 'Failed to set primary contact.');
      },
    });
  }

  delete(contact: EmergencyContactDto): void {
    this.confirm
      .confirm({
        title: 'Delete contact',
        message: `Delete the emergency contact "${contact.contactName}"? This action cannot be undone.`,
        confirmLabel: 'Delete',
        danger: true,
      })
      .subscribe((ok) => {
        if (!ok) return;
        this.service.deleteContact(contact.id).subscribe({
          next: (res) => {
            if (res.success) {
              this.toast.success('Contact removed.');
              this.contacts.set(this.contacts().filter((c) => c.id !== contact.id));
            } else {
              this.toast.error(res.message || 'Failed to delete contact.');
            }
          },
          error: (err: HttpErrorResponse) => {
            this.toast.error(err.error?.message || 'Failed to delete contact.');
          },
        });
      });
  }

  drawerOpen = () => this.drawer() !== 'closed';
  editingContact(): EmergencyContactDto | null {
    const d = this.drawer();
    return typeof d === 'object' ? d.contact : null;
  }
}
