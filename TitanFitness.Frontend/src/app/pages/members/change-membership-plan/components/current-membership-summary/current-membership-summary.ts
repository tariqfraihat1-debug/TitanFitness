import { Component, computed, input } from '@angular/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faLock } from '@fortawesome/free-solid-svg-icons';
import { MembershipDetailsDto } from '../../../../../apis/memberships/membership-details.dto';

@Component({
  selector: 'app-current-membership-summary',
  standalone: true,
  imports: [FontAwesomeModule],
  templateUrl: './current-membership-summary.html'
})
export class CurrentMembershipSummary {
  membership = input.required<MembershipDetailsDto>();

  readonly lockIcon = faLock;

  readonly details = computed(() => {
    const membership = this.membership();

    return [
      { label: 'Plan', value: membership.planName },
      { label: 'Price paid', value: membership.pricePaid.toFixed(2) },
      { label: 'Duration', value: `${membership.durationInMonths} months` },
      { label: 'Access scope', value: membership.accessScope },
      { label: 'Max freeze days', value: membership.maximumFreezeDays.toString() },
      { label: 'Max number of freezes', value: membership.maximumNumberOfFreezes.toString() },
      { label: 'Guest pass quota', value: membership.guestPassQuota.toString() },
      {
        label: 'Start / End',
        value: `${this.formatDate(membership.startDate)} → ${this.formatDate(membership.endDate)}`
      }
    ];
  });

  private formatDate(value: string): string {
    const [year, month, day] = value.split('-').map(Number);

    return new Date(year, month - 1, day).toLocaleDateString(
      'en-US',
      { month: 'short', day: 'numeric', year: 'numeric' }
    );
  }
} 
