import { Component } from '@angular/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faCircleInfo } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-info-notice',
  standalone: true,
  imports: [FontAwesomeModule],
  templateUrl: './info-notice.html'
})
export class InfoNotice {
  readonly icon = faCircleInfo;
}
