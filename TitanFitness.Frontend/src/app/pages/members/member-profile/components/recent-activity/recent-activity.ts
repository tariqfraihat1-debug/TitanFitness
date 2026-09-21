import { DatePipe } from '@angular/common';
import { Component, input } from '@angular/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { IconDefinition } from '@fortawesome/fontawesome-svg-core';
import {
  faBolt,
  faRightToBracket
} from '@fortawesome/free-solid-svg-icons';
import { MemberActivityDto } from '../../../../../apis/members/member-activity.dto';
import { EmptyCard } from '../../../../../shared/components/empty-card/empty-card';
@Component({
  selector: 'app-recent-activity',
  standalone: true,
  imports: [DatePipe, FontAwesomeModule, EmptyCard],
  templateUrl: './recent-activity.html'
})
export class RecentActivity {
  activities = input.required<MemberActivityDto[]>();

  readonly checkInIcon = faRightToBracket;
  readonly classIcon = faBolt;

  getIcon(activityType: string): IconDefinition {
    return activityType.toLowerCase().includes('class')
      ? this.classIcon
      : this.checkInIcon;
  }
}
