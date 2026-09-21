import { Component, input, output } from '@angular/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faEllipsisVertical } from '@fortawesome/free-solid-svg-icons';
import { Button } from '../button/button';

export interface ActionMenuItem {
  label: string;
  value: string;
}

@Component({
  selector: 'app-action-menu',
  standalone: true,
  imports: [FontAwesomeModule, Button],
  templateUrl: './action-menu.html'
})
export class ActionMenu {
  actions = input.required<ActionMenuItem[]>();
  actionSelected = output<string>();

  readonly faEllipsisVertical = faEllipsisVertical;
}
