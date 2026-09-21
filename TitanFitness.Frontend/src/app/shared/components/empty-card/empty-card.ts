import { Component, input, output } from '@angular/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { IconDefinition } from '@fortawesome/fontawesome-svg-core';
import { Button } from '../button/button';

@Component({
  selector: 'app-empty-card',
  standalone: true,
  imports: [FontAwesomeModule, Button],
  templateUrl: './empty-card.html'
})
export class EmptyCard {
  icon = input<IconDefinition | null>(null);
  title = input.required<string>();
  message = input.required<string>();
  buttonText = input('');
  showButton = input(false);
  buttonClick = output<void>();
}
