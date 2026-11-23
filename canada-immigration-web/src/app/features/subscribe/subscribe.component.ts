import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CategoryLabelPipe } from '../../core/pipes/category-label.pipe';
import { SubscriptionService } from '../../core/services/subscription.service';
import { ALL_CATEGORIES_VALUE } from '../../models/subscription.model';

@Component({
  selector: 'app-subscribe',
  standalone: true,
  imports: [CommonModule, FormsModule, CategoryLabelPipe],
  templateUrl: './subscribe.component.html',
  styleUrl: './subscribe.component.scss'
})
export class SubscribeComponent implements OnInit {
  readonly allCategoriesValue = ALL_CATEGORIES_VALUE;

  email = signal('');
  categories = signal<string[]>([]);
  selectedCategories = signal<string[]>([]);
  isLoading = signal(false);
  isLoadingCategories = signal(false);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  constructor(private subscriptionService: SubscriptionService) {}

  ngOnInit(): void {
    this.isLoadingCategories.set(true);
    this.subscriptionService.getCategories().subscribe({
      next: (categories) => {
        this.categories.set(categories);
        this.isLoadingCategories.set(false);
      },
      error: (err) => {
        console.error('Erro ao buscar categorias', err);
        this.isLoadingCategories.set(false);
      }
    });
  }

  isCategorySelected(category: string): boolean {
    return this.selectedCategories().includes(category);
  }

  toggleCategory(category: string): void {
    const current = this.selectedCategories();

    if (category === this.allCategoriesValue) {
      this.selectedCategories.set(current.includes(category) ? [] : [category]);
      return;
    }

    const withoutAll = current.filter((c) => c !== this.allCategoriesValue);
    const next = withoutAll.includes(category)
      ? withoutAll.filter((c) => c !== category)
      : [...withoutAll, category];

    this.selectedCategories.set(next);
  }

  subscribe(): void {
    this.errorMessage.set(null);
    this.successMessage.set(null);

    if (!this.email().trim()) {
      this.errorMessage.set('Informe um e-mail.');
      return;
    }

    if (this.selectedCategories().length === 0) {
      this.errorMessage.set('Selecione ao menos uma categoria (ou "Todas/Qualquer").');
      return;
    }

    this.isLoading.set(true);

    this.subscriptionService.subscribe({
      email: this.email().trim(),
      categories: this.selectedCategories()
    }).subscribe({
      next: (res) => {
        this.successMessage.set(res.message);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Erro ao criar inscrição', err);
        this.errorMessage.set(err?.error ?? 'Não foi possível concluir a inscrição. Tente novamente.');
        this.isLoading.set(false);
      }
    });
  }
}
