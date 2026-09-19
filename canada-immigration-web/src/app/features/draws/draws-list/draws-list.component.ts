import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CategoryLabelPipe } from '../../../core/pipes/category-label.pipe';
import { ExpressEntryDrawService } from '../../../core/services/express-entry-draw.service';
import { ExpressEntryDraw, PagedResult } from '../../../models/express-entry-draw.model';

const FIRST_EXPRESS_ENTRY_YEAR = 2015;

@Component({
  selector: 'app-draws-list',
  standalone: true,
  imports: [CommonModule, FormsModule, CategoryLabelPipe],
  templateUrl: './draws-list.component.html',
  styleUrl: './draws-list.component.scss'
})
export class DrawsListComponent implements OnInit {
  draws = signal<ExpressEntryDraw[]>([]);
  latestDraw = signal<ExpressEntryDraw | null>(null);
  currentPage = signal(1);
  totalPages = signal(1);
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);

  categories = signal<string[]>([]);
  selectedYear = signal<number | null>(null);
  selectedCategory = signal<string | null>(null);

  readonly years: number[] = (() => {
    const currentYear = new Date().getFullYear();
    return Array.from({ length: currentYear - FIRST_EXPRESS_ENTRY_YEAR + 1 }, (_, i) => currentYear - i);
  })();

  constructor(private drawService: ExpressEntryDrawService) {}

  ngOnInit(): void {
    this.loadPage(1);
    this.loadLatestDraw();
    this.loadCategories();
  }

  loadCategories(): void {
    this.drawService.getCategories().subscribe({
      next: (categories) => this.categories.set(categories),
      error: (err) => console.error('Erro ao buscar categorias', err)
    });
  }

  onFilterChange(): void {
    this.loadPage(1);
  }

  loadPage(page: number): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.drawService.getDraws(page, 20, this.selectedYear() ?? undefined, this.selectedCategory() ?? undefined).subscribe({
      next: (result: PagedResult<ExpressEntryDraw>) => {
        this.draws.set(result.items);
        this.currentPage.set(result.page);
        this.totalPages.set(result.totalPages);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Erro ao buscar draws', err);
        this.errorMessage.set('Não foi possível carregar os dados. Tente novamente.');
        this.isLoading.set(false);
      }
    });
  }

  loadLatestDraw(): void {
    this.drawService.getLatestDraw().subscribe({
      next: (draw) => this.latestDraw.set(draw),
      error: (err) => console.error('Erro ao buscar o último draw', err)
    });
  }

  goToPreviousPage(): void {
    if (this.currentPage() > 1) {
      this.loadPage(this.currentPage() - 1);
    }
  }

  goToNextPage(): void {
    if (this.currentPage() < this.totalPages()) {
      this.loadPage(this.currentPage() + 1);
    }
  }
}
