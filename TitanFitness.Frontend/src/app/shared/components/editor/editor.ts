import { Component, input } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-editor',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './editor.html'
})
export class Editor {
  label = input.required<string>();

  control = input.required<
    FormControl<string | null> |
    FormControl<number | null>
  >();

  type = input<'text' | 'email' | 'tel' | 'date' | 'number'>('text');

  placeholder = input('');
  helper = input('');
  requiredMark = input(false);
  readonly = input(false);
  errorMessage = input('');

  min = input<number | null>(null);
  step = input<number | string | null>(null);
}
