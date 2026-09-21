import { DecimalPipe } from '@angular/common';
import { Component, input, output } from '@angular/core';
import { PlanLookupItemDto } from '../../../../../apis/memberships/plan-lookup-item.dto';

@Component({
  selector: 'app-plan-selector',
  standalone: true,
  imports: [DecimalPipe],
  templateUrl: './plan-selector.html'
})
export class PlanSelector {
  plans = input.required<PlanLookupItemDto[]>();
  selectedPlanId = input<number | null>(null);
  selectedPlanIdChange = output<number | null>();

  onChange(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;

    this.selectedPlanIdChange.emit(value ? Number(value) : null);
  }
}
