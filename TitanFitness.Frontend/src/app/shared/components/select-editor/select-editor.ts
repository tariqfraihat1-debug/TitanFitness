import { Component, input } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-select-editor',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './select-editor.html'
})
export class SelectEditor {
  label = input.required<string>();
  control = input.required<FormControl<number | null>>();
  options = input.required<{ value: number; label: string }[]>();
  placeholder = input('Select option');
  helper = input('');
  requiredMark = input(false);
  errorMessage = input('');
}
