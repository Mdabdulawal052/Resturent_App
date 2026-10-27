import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'bdt',
  standalone: true
})
export class BdtCurrencyPipe implements PipeTransform {
  transform(value: number | string | null | undefined): string {
    if (value === null || value === undefined) return '৳0.00';
    const num = typeof value === 'string' ? parseFloat(value) : value;
    if (isNaN(num)) return '৳0.00';
    return '৳' + num.toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  }
}
