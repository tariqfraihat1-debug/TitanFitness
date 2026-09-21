import { Component, computed, input } from '@angular/core';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  imports: [],
  templateUrl: './status-badge.html'
})
export class StatusBadge {
  status = input.required<string>();

  badgeClass = computed(() => {
    switch (this.status().toLowerCase()) {
      case 'active':
      case 'open':
      case 'booked':
      case 'attended':
      case 'admitted':
      case 'upcoming':
        return 'bg-success-subtle text-success';

      case 'published':
      case 'pending':
      case 'frozen':
      case 'in progress':
      case 'waitlisted':
        return 'bg-warning-subtle text-warning-emphasis';

      case 'expired':
      case 'cancelled':
      case 'refused':
      case 'no show':
      case 'retired':
        return 'bg-danger-subtle text-danger';

      default:
        return 'bg-secondary-subtle text-secondary';
    }
  });
}
