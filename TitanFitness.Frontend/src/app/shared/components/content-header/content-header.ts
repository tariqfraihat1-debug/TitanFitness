import { Component, input } from '@angular/core';

@Component({
  selector: 'app-content-header',
  standalone: true,
  imports: [],
  templateUrl: './content-header.html'
})
export class ContentHeader {
  title = input.required<string>();
  subtitle = input('');
}
