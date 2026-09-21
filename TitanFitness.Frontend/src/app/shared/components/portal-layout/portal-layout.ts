import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { CheckIn } from '../../../pages/check-in/check-in';
import { CheckInDialogService } from '../../services/check-in-dialog.service';
import { Sidebar } from '../sidebar/sidebar';
import { TopBar } from '../top-bar/top-bar';

@Component({
  selector: 'app-portal-layout',
  standalone: true,
  imports: [
    RouterOutlet,
    Sidebar,
    TopBar,
    CheckIn
  ],
  templateUrl: './portal-layout.html'
})
export class PortalLayout {
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly checkInDialog = inject(CheckInDialogService);

  showSearch = signal(false);
  searchPlaceholder = signal('Search...');

  constructor() {
    this.updateTopBar(this.router.url);

    this.router.events
      .pipe(
        filter(event => event instanceof NavigationEnd),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(event =>
        this.updateTopBar(event.urlAfterRedirects)
      );
  }

  onSearch(value: string): void {
    const url = this.router.url.split('?')[0];

    this.router.navigate([url], {
      queryParams: {
        search: value || null,
        page: 1
      },
      queryParamsHandling: 'merge'
    });
  }

  private updateTopBar(url: string): void {
    const path = url.split('?')[0];

    if (path === '/members') {
      this.showSearch.set(true);
      this.searchPlaceholder.set('Search members...');
      return;
    }

    if (path === '/trainers') {
      this.showSearch.set(true);
      this.searchPlaceholder.set('Search trainers...');
      return;
    }

    if (path === '/plans') {
      this.showSearch.set(true);
      this.searchPlaceholder.set('Search plans...');
      return;
    }

    this.showSearch.set(false);
    this.searchPlaceholder.set('Search...');
  }
}
