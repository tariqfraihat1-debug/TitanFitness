import { Component, input } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Editor } from '../../../../../shared/components/editor/editor';
import { AccessScopeFilter } from '../../../../../apis/plans/access-scope-filter';
import { PlanTermsOfferedDto } from '../../../../../apis/plans/plan-terms-offered.dto';


type TermsEditorControl = Exclude<
  keyof PlanTermsOfferedDto<number>,
  'accessScope'
>;

@Component({
  selector: 'app-plan-terms-offered',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    Editor
  ],
  templateUrl: './plan-terms-offered.html'
})
export class PlanTermsOffered {
  form = input.required<FormGroup>();
  viewMode = input(false);

  readonly AccessScopeFilter = AccessScopeFilter;

  readonly fields: {
    control: TermsEditorControl;
    label: string;
    placeholder: string;
    errorMessage: string;
  }[] = [
      {
        control: 'maxFreezeDays',
        label: 'Maximum freeze days',
        placeholder: '0',
        errorMessage: 'Maximum freeze days cannot be negative.'
      },
      {
        control: 'maxFreezes',
        label: 'Maximum number of freezes',
        placeholder: '0',
        errorMessage: 'Maximum number of freezes cannot be negative.'
      },
      {
        control: 'guestPassQuota',
        label: 'Guest pass quota',
        placeholder: '0',
        errorMessage: 'Guest pass quota cannot be negative.'
      }
    ];

  control(name: TermsEditorControl): FormControl<number | null> {
    return this.form().get(name) as FormControl<number | null>;
  }

  accessScopeControl(): FormControl<number | null> {
    return this.form().get('accessScope') as FormControl<number | null>;
  }

  accessScopeLabel(): string {
    return this.accessScopeControl().value === AccessScopeFilter.HomeBranchOnly
      ? 'Home branch only'
      : this.accessScopeControl().value === AccessScopeFilter.AllBranches
        ? 'All branches'
        : '';
  }
}
