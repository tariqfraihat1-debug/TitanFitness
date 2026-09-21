import { Pipe, PipeTransform } from '@angular/core';

export type Time12HourPart = 'time' | 'period' | 'full';

@Pipe({
  name: 'time12Hour',
  standalone: true
})
export class Time12HourPipe implements PipeTransform {

  // Converts a 24-hour time into a 12-hour display value.
  transform(
    value: string,
    part: Time12HourPart = 'full'
  ): string {
    if (!value)
      return '';

    const [hour, minute] = value
      .split(':')
      .map(Number);

    const period = hour >= 12
      ? 'PM'
      : 'AM';

    const displayHour = hour % 12 || 12;

    const time =
      `${String(displayHour).padStart(2, '0')}:${String(minute).padStart(2, '0')}`;

    if (part === 'time')
      return time;

    if (part === 'period')
      return period;

    return `${time} ${period}`;
  }
}
