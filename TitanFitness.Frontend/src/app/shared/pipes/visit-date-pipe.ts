import { formatDate } from '@angular/common';
import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'visitDate',
  standalone: true
})
export class VisitDatePipe implements PipeTransform {
  transform(value: string | null | undefined): string {
    if (!value)
      return 'Never';

    const date = new Date(value);

    if (Number.isNaN(date.getTime()))
      return value;

    const today = new Date();
    const visitDate = new Date(date.getFullYear(), date.getMonth(), date.getDate());
    const currentDate = new Date(today.getFullYear(), today.getMonth(), today.getDate());
    const difference = Math.round((currentDate.getTime() - visitDate.getTime()) / 86400000);

    if (difference === 0)
      return `Today, ${formatDate(date, 'hh:mm a', 'en-US')}`;

    if (difference === 1)
      return 'Yesterday';

    return formatDate(date, 'MMM dd, yyyy', 'en-US');
  }
}
