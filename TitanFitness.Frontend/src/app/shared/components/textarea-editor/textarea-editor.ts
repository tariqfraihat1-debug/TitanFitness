import { Component, input } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-textarea-editor',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './textarea-editor.html'
})
export class TextareaEditor {
  id = input.required<string>();
  label = input.required<string>();
  control = input.required<FormControl<string | null>>();
  placeholder = input('');
  rows = input(3);
  optionalText = input('');
  requiredMark = input(false);
  maxLength = input<number | null>(null);
  errorMessage = input('');
}
