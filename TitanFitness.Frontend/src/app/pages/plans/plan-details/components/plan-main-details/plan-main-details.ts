import { Component, input } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { PlanMainDetailsDto } from '../../../../../apis/plans/plan-main-details.dto';
import { Editor } from '../../../../../shared/components/editor/editor';


type MainEditorControl = Exclude<
  keyof PlanMainDetailsDto,
  'isPublished'
>;

type MainFormControl =
  FormControl<string | null> |
  FormControl<number | null>;

@Component({
  selector: 'app-plan-main-details',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    Editor
  ],
  templateUrl: './plan-main-details.html'
})
export class PlanMainDetails {
  form = input.required<FormGroup>();
  viewMode = input(false);

  readonly fields: {
    control: MainEditorControl;
    label: string;
    type: 'text' | 'number';
    placeholder: string;
    requiredMark: boolean;
    errorMessage: string;
    min: number | null;
    step: number | string | null;
  }[] = [
      {
        control: 'planName',
        label: 'Plan name',
        type: 'text',
        placeholder: 'e.g., Annual Pro',
        requiredMark: true,
        errorMessage: 'Plan name is required and cannot exceed 50 characters.',
        min: null,
        step: null
      },
      {
        control: 'price',
        label: 'Price',
        type: 'number',
        placeholder: '0.00',
        requiredMark: true,
        errorMessage: 'Price must be zero or greater.',
        min: 0,
        step: '0.01'
      },
      {
        control: 'durationInMonths',
        label: 'Duration in months',
        type: 'number',
        placeholder: 'e.g., 12',
        requiredMark: true,
        errorMessage: 'Duration must be greater than zero.',
        min: 1,
        step: 1
      }
    ];

  control(name: MainEditorControl): MainFormControl {
    return this.form().get(name) as MainFormControl;
  }

  publishedControl(): FormControl<boolean> {
    return this.form().get('isPublished') as FormControl<boolean>;
  }
}
