import { CommonModule } from '@angular/common';
import { Component, HostListener, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { ToastContainerComponent } from '../../shared/components/toast-container/toast-container.component';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { TopbarComponent } from '../topbar/topbar.component';

@Component({
  selector: 'hrm-shell',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    SidebarComponent,
    TopbarComponent,
    ToastContainerComponent,
    ConfirmDialogComponent,
  ],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.scss',
})
export class ShellComponent {
  readonly sidebarCollapsed = signal(false);
  readonly mobileNavOpen = signal(false);
  readonly isMobile = signal(typeof window !== 'undefined' ? window.innerWidth < 768 : false);

  toggleSidebar(): void {
    if (this.isMobile()) {
      this.mobileNavOpen.update((v) => !v);
    } else {
      this.sidebarCollapsed.update((v) => !v);
    }
  }

  closeMobileNav(): void {
    this.mobileNavOpen.set(false);
  }

  @HostListener('window:resize')
  onResize(): void {
    if (typeof window === 'undefined') return;
    const mobile = window.innerWidth < 768;
    this.isMobile.set(mobile);
    if (!mobile) this.mobileNavOpen.set(false);
  }
}
