import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-modal',
  standalone: true,
  imports: [],
  templateUrl: './modal.html'
})
export class Modal {
  title = input.required<string>();
  closeDisabled = input(false);
  closed = output<void>();
}
