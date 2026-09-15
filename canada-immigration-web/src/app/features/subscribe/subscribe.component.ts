import { Component, ElementRef, OnInit, ViewChild, signal } from '@angular/core';
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

  @ViewChild('categorySelect') categorySelect?: ElementRef<HTMLSelectElement>;

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

  onCategoriesChange(event: Event): void {
    const select = event.target as HTMLSelectElement;
    const selected = Array.from(select.selectedOptions).map((option) => option.value);

    // Se o usuário marcou "Todas/Qualquer", as outras seleções ficam redundantes,
    // então limpamos o select visualmente para refletir só a escolha "ALL".
    if (selected.includes(this.allCategoriesValue) && selected.length > 1) {
      Array.from(select.options).forEach((option) => {
        option.selected = option.value === this.allCategoriesValue;
      });
      this.selectedCategories.set([this.allCategoriesValue]);
      return;
    }

    this.selectedCategories.set(selected);
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
