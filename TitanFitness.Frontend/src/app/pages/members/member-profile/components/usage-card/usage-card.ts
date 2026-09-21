import { Component, computed, input } from '@angular/core';

@Component({
  selector: 'app-usage-card',
  standalone: true,
  imports: [],
  templateUrl: './usage-card.html'
})
export class UsageCard {
  title = input.required<string>();
  used = input.required<number>();
  allowed = input.required<number>();
  itemName = input.required<string>();

  remaining = computed(() =>
    Math.max(this.allowed() - this.used(), 0)
  );

  progressPercent = computed(() => {
    if (this.allowed() <= 0 || this.used() <= 0)
      return 0;

    const percentage =
      (this.used() / this.allowed()) * 100;

    return Math.min(percentage, 100);
  });
}
