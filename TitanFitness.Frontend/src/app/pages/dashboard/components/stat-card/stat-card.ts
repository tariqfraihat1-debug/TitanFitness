import { Component, input } from '@angular/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { IconDefinition } from '@fortawesome/fontawesome-svg-core';

@Component({
  selector: 'app-stat-card',
  standalone: true,
  imports: [FontAwesomeModule],
  templateUrl: './stat-card.html'
})
export class StatCard {
  title = input.required<string>();
  value = input.required<string | number>();
  subtitle = input('');
  icon = input.required<IconDefinition>();
}
