import { CommonModule } from '@angular/common';
import { Component, computed, inject } from '@angular/core';
import { ActivationEnd, Router, RouterLink } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { filter, map, startWith } from 'rxjs/operators';

import { NAV_ITEMS, NavItem } from '../sidebar/nav-items';

interface Crumb {
  label: string;
  path?: string;
}

@Component({
  selector: 'hrm-breadcrumb',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <nav aria-label="Breadcrumb" class="breadcrumb">
      <ol>
        <ng-container *ngFor="let crumb of crumbs(); let last = last; let i = index">
          <li>
            <a *ngIf="!last && crumb.path" [routerLink]="crumb.path">{{ crumb.label }}</a>
            <span *ngIf="last || !crumb.path" [class.breadcrumb__current]="last">{{ crumb.label }}</span>
          </li>
          <li *ngIf="!last" aria-hidden="true" class="breadcrumb__sep">/</li>
        </ng-container>
      </ol>
    </nav>
  `,
  styleUrl: './breadcrumb.component.scss',
})
export class BreadcrumbComponent {
  private readonly router = inject(Router);

  private readonly currentUrl = toSignal(
    this.router.events.pipe(
      filter((e) => e instanceof ActivationEnd),
      map(() => this.router.url),
      startWith(this.router.url),
    ),
    { initialValue: this.router.url },
  );

  readonly crumbs = computed<Crumb[]>(() => this.buildCrumbs(this.currentUrl()));

  private buildCrumbs(url: string): Crumb[] {
    const path = url.split('?')[0].split('#')[0];
    const match = this.findInNav(NAV_ITEMS, path);
    if (!match) return [{ label: 'Dashboard', path: '/dashboard' }];
    return match;
  }

  private findInNav(items: NavItem[], path: string, parents: Crumb[] = []): Crumb[] | null {
    for (const item of items) {
      if (item.path && (path === item.path || path.startsWith(item.path + '/'))) {
        return [...parents, { label: item.label, path: item.path }];
      }
      if (item.children) {
        const nested = this.findInNav(item.children, path, [...parents, { label: item.label }]);
        if (nested) return nested;
      }
    }
    return null;
  }
}
