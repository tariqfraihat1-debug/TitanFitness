import { Component, input } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-freeze-parameters',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './freeze-parameters.html'
})
export class FreezeParameters {
  startDate = input.required<FormControl<string | null>>();
  freezeDurationId = input.required<FormControl<number | null>>();
  freezeReasonId = input.required<FormControl<number | null>>();
  notes = input.required<FormControl<string | null>>();

  durations = input.required<ReadonlyArray<{
    id: number;
    label: string;
    months: number;
  }>>();

  reasons = input.required<ReadonlyArray<{
    id: number;
    label: string;
  }>>();

  minStartDate = input.required<string>();

  selectDuration(id: number): void {
    this.freezeDurationId().setValue(id);
    this.freezeDurationId().markAsTouched();
  }
}
