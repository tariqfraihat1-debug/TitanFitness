import { Component, computed, input, output } from '@angular/core';
import { Button } from '../button/button';

@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [Button],
  templateUrl: './pagination.html'
})
export class Pagination {
  currentPage = input.required<number>();
  totalPages = input.required<number>();
  pageChanged = output<number>();

  pages = computed(() => Array.from({ length: this.totalPages() }, (_, index) => index + 1));

  changePage(page: number): void {
    if (page < 1 || page > this.totalPages() || page === this.currentPage())
      return;

    this.pageChanged.emit(page);
  }
}
