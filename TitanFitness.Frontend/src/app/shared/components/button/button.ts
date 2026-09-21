import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-button',
  standalone: true,
  imports: [],
  templateUrl: './button.html'
})
export class Button {
  variant = input<'primary' | 'secondary' | 'outline-primary' | 'outline-secondary' | 'light' | 'link'>('primary');
  type = input<'button' | 'submit' | 'reset'>('button');
  disabled = input(false);
  clicked = output<void>();
}
