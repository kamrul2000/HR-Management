import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { NgIcon, provideIcons } from '@ng-icons/core';
import {
  heroUsers,
  heroBriefcase,
  heroBanknotes,
  heroDocumentText,
  heroChartBar,
  heroArrowTrendingUp,
  heroArrowTrendingDown,
} from '@ng-icons/heroicons/outline';

export type StatTone = 'primary' | 'success' | 'warning' | 'danger' | 'info';
export type StatTrend = { direction: 'up' | 'down'; value: string };

@Component({
  selector: 'hrm-stat-card',
  standalone: true,
  imports: [CommonModule, NgIcon],
  providers: [
    provideIcons({
      heroUsers,
      heroBriefcase,
      heroBanknotes,
      heroDocumentText,
      heroChartBar,
      heroArrowTrendingUp,
      heroArrowTrendingDown,
    }),
  ],
  templateUrl: './stat-card.component.html',
  styleUrl: './stat-card.component.scss',
})
export class StatCardComponent {
  @Input({ required: true }) label!: string;
  @Input({ required: true }) value!: string | number;
  @Input() icon: string = 'heroChartBar';
  @Input() tone: StatTone = 'primary';
  @Input() trend?: StatTrend;
  @Input() loading = false;
}
