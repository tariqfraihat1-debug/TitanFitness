import { Component, input, output } from '@angular/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faArrowLeft } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-back-navigation',
  standalone: true,
  imports: [FontAwesomeModule],
  templateUrl: './back-navigation.html'
})
export class BackNavigation {
  label = input.required<string>();
  clicked = output<void>();
  readonly icon = faArrowLeft;
}
