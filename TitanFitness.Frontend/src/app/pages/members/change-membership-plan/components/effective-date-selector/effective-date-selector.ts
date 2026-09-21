import { Component, input, output } from '@angular/core';
import { ChangePlanEffectiveMode } from '../../../../../apis/memberships/change-plan-effective-mode';

@Component({
  selector: 'app-effective-date-selector',
  standalone: true,
  imports: [],
  templateUrl: './effective-date-selector.html'
})
export class EffectiveDateSelector {
  mode = input.required<ChangePlanEffectiveMode>();
  startDate = input.required<string>();
  modeChange = output<ChangePlanEffectiveMode>();

  readonly effectiveMode = ChangePlanEffectiveMode;
}
