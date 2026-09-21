import { Component, input, output } from '@angular/core';
import { MemberPhoto } from '../member-photo/member-photo';
import { StatusBadge } from '../status-badge/status-badge';

@Component({
  selector: 'app-member-summary-card',
  standalone: true,
  imports: [
    MemberPhoto,
    StatusBadge
  ],
  templateUrl: './member-summary-card.html'
})
export class MemberSummaryCard {
  name = input.required<string>();
  membershipNumber = input.required<string>();
  status = input.required<string>();
  photo = input<string | null>(null);
  selectable = input(false);

  selected = output<void>();

  select(): void {
    if (this.selectable())
      this.selected.emit();
  }
}
