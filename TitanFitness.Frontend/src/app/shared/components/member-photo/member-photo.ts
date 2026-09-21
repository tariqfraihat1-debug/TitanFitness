import { Component, input } from '@angular/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faUser } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-member-photo',
  standalone: true,
  imports: [FontAwesomeModule],
  templateUrl: './member-photo.html',
  host: {
    class: 'd-block w-100 h-100'
  }
})
export class MemberPhoto {
  photo = input<string | null>(null);
  alt = input('Member photo');

  readonly icon = faUser;
}
