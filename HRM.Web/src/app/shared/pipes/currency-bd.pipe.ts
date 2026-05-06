import { Pipe, PipeTransform } from '@angular/core';

/**
 * Formats a number as Bangladeshi Taka with the standard thousands separator.
 * Use [showSymbol]="false" to suppress the prefix.
 */
@Pipe({
  name: 'currencyBd',
  standalone: true,
  pure: true,
})
export class CurrencyBdPipe implements PipeTransform {
  transform(value: number | string | null | undefined, showSymbol = true): string {
    if (value === null || value === undefined || value === '') return '';
    const numeric = typeof value === 'string' ? Number(value) : value;
    if (Number.isNaN(numeric)) return '';

    const formatted = new Intl.NumberFormat('en-IN', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    }).format(numeric);

    return showSymbol ? `৳ ${formatted}` : formatted;
  }
}
