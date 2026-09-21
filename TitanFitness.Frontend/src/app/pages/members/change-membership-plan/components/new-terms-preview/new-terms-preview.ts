import { Component, computed, input, output } from '@angular/core';
import { PlanDetailsDto } from '../../../../../apis/plans/plan-details.dto';
import { Button } from '../../../../../shared/components/button/button';
import { InfoNotice } from '../../../../../shared/components/info-notice/info-notice';

@Component({
  selector: 'app-new-terms-preview',
  standalone: true,
  imports: [Button, InfoNotice],
  templateUrl: './new-terms-preview.html'
})
export class NewTermsPreview {
  plan = input<PlanDetailsDto | null>(null);
  newEndDate = input('');
  saving = input(false);

  confirm = output<void>();
  cancel = output<void>();

  readonly details = computed(() => {
    const plan = this.plan();

    if (!plan)
      return [];

    return [
      { label: 'Plan', value: plan.planName },
      { label: 'Price', value: plan.price.toFixed(2) },
      { label: 'Duration', value: `${plan.durationInMonths} months` },
      { label: 'Max freeze days', value: plan.maxFreezeDays.toString() },
      { label: 'Max freezes', value: plan.maxFreezes.toString() },
      { label: 'Guest passes', value: plan.guestPassQuota.toString() },
      { label: 'Access scope', value: plan.accessScope },
      { label: 'New end date', value: this.formatDate(this.newEndDate()) }
    ];
  });

  private formatDate(value: string): string {
    if (!value)
      return '—';

    const [year, month, day] = value.split('-').map(Number);

    return new Date(year, month - 1, day).toLocaleDateString(
      'en-US',
      { month: 'short', day: 'numeric', year: 'numeric' }
    );
  }
}
