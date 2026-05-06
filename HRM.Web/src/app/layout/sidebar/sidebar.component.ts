import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { NgIcon, provideIcons } from '@ng-icons/core';
import {
  heroChevronDown,
  heroBars3,
  heroXMark,
} from '@ng-icons/heroicons/outline';

import { NAV_ICONS, NAV_ITEMS, NavItem } from './nav-items';

@Component({
  selector: 'hrm-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, NgIcon],
  providers: [provideIcons({ ...NAV_ICONS, heroChevronDown, heroBars3, heroXMark })],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.scss',
})
export class SidebarComponent {
  @Input() collapsed = false;
  @Input() mobileOpen = false;
  @Output() mobileClose = new EventEmitter<void>();

  readonly items = NAV_ITEMS;
  private readonly _expanded = signal<Record<string, boolean>>({});
  readonly expanded = this._expanded.asReadonly();

  toggle(item: NavItem): void {
    if (!item.children) return;
    const current = this._expanded();
    this._expanded.set({ ...current, [item.label]: !current[item.label] });
  }

  isExpanded(item: NavItem): boolean {
    return !!this._expanded()[item.label];
  }

  trackItem = (_: number, item: NavItem) => item.label;
}
