import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CategoryLabelPipe } from '../../../core/pipes/category-label.pipe';
import { ExpressEntryDrawService } from '../../../core/services/express-entry-draw.service';
import { ExpressEntryDraw, PagedResult } from '../../../models/express-entry-draw.model';

@Component({
  selector: 'app-draws-list',
  standalone: true,
  imports: [CommonModule, CategoryLabelPipe],
  templateUrl: './draws-list.component.html',
  styleUrl: './draws-list.component.scss'
})
export class DrawsListComponent implements OnInit {
  draws = signal<ExpressEntryDraw[]>([]);
  currentPage = signal(1);
  totalPages = signal(1);
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);

  constructor(private drawService: ExpressEntryDrawService) {}

  ngOnInit(): void {
    this.loadPage(1);
  }

  loadPage(page: number): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.drawService.getDraws(page).subscribe({
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
