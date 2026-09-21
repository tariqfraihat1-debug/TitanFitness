import { Component, input } from '@angular/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import {
  faCircleCheck,
  faCircleXmark
} from '@fortawesome/free-solid-svg-icons';
import { EntryEligibilityDto } from '../../../../apis/members/entry-eligibility.dto';

@Component({
  selector: 'app-entry-eligibility',
  standalone: true,
  imports: [FontAwesomeModule],
  templateUrl: './entry-eligibility.html'
})
export class EntryEligibility {
  eligibility = input.required<EntryEligibilityDto>();
  branchName = input.required<string>();

  readonly admittedIcon = faCircleCheck;
  readonly refusedIcon = faCircleXmark;
}
