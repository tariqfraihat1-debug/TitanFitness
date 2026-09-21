import { Component, output } from '@angular/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { IconDefinition } from '@fortawesome/fontawesome-svg-core';
import { faCalendarCheck, faRightToBracket, faUserPlus } from '@fortawesome/free-solid-svg-icons';
import { Button } from '../../../../shared/components/button/button';

export type QuickAction = 'newMember' | 'checkIn' | 'registerClass';

@Component({
  selector: 'app-quick-actions',
  standalone: true,
  imports: [FontAwesomeModule, Button],
  templateUrl: './quick-actions.html'
})
export class QuickActions {
  actionSelected = output<QuickAction>();

  readonly actions: { title: string; description: string; icon: IconDefinition; action: QuickAction }[] = [
    { title: 'New Member', description: 'Start enrollment process', icon: faUserPlus, action: 'newMember' },
    { title: 'Manual Check-In', description: 'Verify member entry', icon: faRightToBracket, action: 'checkIn' },
    { title: 'Register Class', description: 'Book member into session', icon: faCalendarCheck, action: 'registerClass' }
  ];
}
