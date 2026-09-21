import { Component, DestroyRef, inject, input, OnInit, output, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router } from '@angular/router';
import { filter } from 'rxjs';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { IconDefinition } from '@fortawesome/fontawesome-svg-core';
import { faBars, faBell, faCalendarDays, faCircleUser, faMagnifyingGlass } from '@fortawesome/free-solid-svg-icons';
import { Button } from '../button/button';

@Component({
  selector: 'app-top-bar',
  standalone: true,
  imports: [FontAwesomeModule, Button],
  templateUrl: './top-bar.html'
})
export class TopBar implements OnInit {
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  showSearch = input(false);
  searchPlaceholder = input('Search...');
  searchChanged = output<string>();

  searchValue = signal('');

  private currentPath = '';

  readonly faBars = faBars;
  readonly faMagnifyingGlass = faMagnifyingGlass;

  readonly actions: { label: string; icon: IconDefinition }[] = [
    { label: 'Notifications', icon: faBell },
    { label: 'Calendar', icon: faCalendarDays },
    { label: 'User profile', icon: faCircleUser }
  ];

  // Tracks route changes and clears search when leaving a page.
  ngOnInit(): void {
    this.currentPath = this.getPath(this.router.url);

    this.router.events
      .pipe(
        filter(
          (event): event is NavigationEnd =>
            event instanceof NavigationEnd
        ),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(event => {
        const path = this.getPath(
          event.urlAfterRedirects
        );

        if (path !== this.currentPath)
          this.searchValue.set('');

        this.currentPath = path;
      });
  }

  // Updates and emits the current search value.
  onSearch(value: string): void {
    this.searchValue.set(value);
    this.searchChanged.emit(value);
  }

  // Returns the route path without query parameters.
  private getPath(url: string): string {
    return url.split('?')[0];
  }
}
