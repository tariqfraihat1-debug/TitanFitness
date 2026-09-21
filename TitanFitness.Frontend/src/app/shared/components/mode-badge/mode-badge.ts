import { Component, input } from '@angular/core';

@Component({
  selector: 'app-mode-badge',
  standalone: true,
  imports: [],
  templateUrl: './mode-badge.html'
})
export class ModeBadge {
  mode = input.required<'add' | 'edit' | 'view'>();
}
