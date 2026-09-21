import { Component, input, output } from '@angular/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faChartColumn, faSnowflake } from '@fortawesome/free-solid-svg-icons';
import { Button } from '../../../../../shared/components/button/button';

@Component({
  selector: 'app-projected-impact',
  standalone: true,
  imports: [FontAwesomeModule, Button],
  templateUrl: './projected-impact.html'
})
export class ProjectedImpact {
  originalEndDate = input.required<string>();
  duration = input('—');
  newEndDate = input('');
  canConfirm = input(false);
  saving = input(false);

  confirm = output<void>();
  cancel = output<void>();

  readonly impactIcon = faChartColumn;
  readonly freezeIcon = faSnowflake;

  formatDate(value: string): string {
    if (!value)
      return '—';

    const [year, month, day] = value.split('-').map(Number);

    return new Date(year, month - 1, day).toLocaleDateString(
      'en-US',
      {
        month: 'short',
        day: 'numeric',
        year: 'numeric'
      }
    );
  }
}
