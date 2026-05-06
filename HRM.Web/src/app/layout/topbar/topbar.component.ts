import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Output, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { NgIcon, provideIcons } from '@ng-icons/core';
import {
  heroBars3,
  heroBell,
  heroChevronDown,
  heroArrowRightOnRectangle,
  heroUser,
} from '@ng-icons/heroicons/outline';

import { AuthService } from '../../core/auth/auth.service';
import { ClickOutsideDirective } from '../../shared/directives/click-outside.directive';
import { BreadcrumbComponent } from '../breadcrumb/breadcrumb.component';

@Component({
  selector: 'hrm-topbar',
  standalone: true,
  imports: [CommonModule, NgIcon, BreadcrumbComponent, ClickOutsideDirective],
  providers: [
    provideIcons({
      heroBars3,
      heroBell,
      heroChevronDown,
      heroArrowRightOnRectangle,
      heroUser,
    }),
  ],
  templateUrl: './topbar.component.html',
  styleUrl: './topbar.component.scss',
})
export class TopbarComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  @Output() menuToggle = new EventEmitter<void>();

  readonly user = this.auth.user;
  readonly menuOpen = signal(false);

  readonly greeting = computed(() => {
    const hour = new Date().getHours();
    if (hour < 12) return 'Good morning';
    if (hour < 17) return 'Good afternoon';
    return 'Good evening';
  });

  toggleMenu(): void {
    this.menuOpen.update((v) => !v);
  }

  closeMenu(): void {
    this.menuOpen.set(false);
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }

  initials(): string {
    const name = this.user()?.name ?? '';
    if (!name) return '?';
    const parts = name.trim().split(/\s+/);
    const first = parts[0]?.charAt(0) ?? '';
    const last = parts.length > 1 ? parts[parts.length - 1].charAt(0) : '';
    return (first + last).toUpperCase();
  }
}
