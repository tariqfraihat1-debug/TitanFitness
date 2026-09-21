import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, computed, input, output } from '@angular/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import {
  faCircleInfo,
  faSnowflake
} from '@fortawesome/free-solid-svg-icons';
import { CurrentMembershipDto } from '../../../../../apis/members/current-membership.dto';
import { Button } from '../../../../../shared/components/button/button';
import { StatusBadge } from '../../../../../shared/components/status-badge/status-badge';

@Component({
  selector: 'app-current-plan',
  standalone: true,
  imports: [
    CurrencyPipe,
    DatePipe,
    FontAwesomeModule,
    Button,
    StatusBadge
  ],
  templateUrl: './current-plan.html'
})
export class CurrentPlan {
  membership = input<CurrentMembershipDto | null>(null);

  changePlan = output<void>();
  freezeMembership = output<void>();
  renewMembership = output<void>();

  readonly infoIcon = faCircleInfo;
  readonly freezeIcon = faSnowflake;

  isExpired = computed(() =>
    this.membership()?.status.toLowerCase() === 'expired'
  );

  canFreeze = computed(() =>
    this.membership()?.status.toLowerCase() === 'active'
  );
}
