import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CategoryLabelPipe } from '../../../core/pipes/category-label.pipe';
import { ExpressEntryDrawService } from '../../../core/services/express-entry-draw.service';
import { ExpressEntryDraw, PagedResult } from '../../../models/express-entry-draw.model';

interface TrendPoint {
  drawNumber: number;
  date: string;
  minimumCrs: number;
  category: string | null;
  x: number;
  y: number;
}

interface AxisTick {
  value: number;
  y: number;
}

interface XLabel {
  date: string;
  x: number;
}

const ALL_DRAWS_PAGE_SIZE = 1000;
const Y_TICK_COUNT = 5;
const X_LABEL_COUNT = 6;

@Component({
  selector: 'app-crs-trend',
  standalone: true,
  imports: [CommonModule, FormsModule, CategoryLabelPipe],
  templateUrl: './crs-trend.component.html',
  styleUrl: './crs-trend.component.scss'
})
export class CrsTrendComponent implements OnInit {
  draws = signal<ExpressEntryDraw[]>([]);
  categories = signal<string[]>([]);
  years = signal<number[]>([]);
  selectedCategory = signal<string | null>(null);
  selectedYear = signal<number | null>(null);
  isLoading = signal(false);
  isLoadingCategories = signal(false);
  errorMessage = signal<string | null>(null);

  readonly chartWidth = 720;
  readonly chartHeight = 340;
  readonly paddingLeft = 56;
  readonly paddingRight = 16;
  readonly paddingTop = 16;
  readonly paddingBottom = 40;

  chart = computed(() => {
    const ordered = [...this.draws()].sort((a, b) => a.date.localeCompare(b.date));
    const n = ordered.length;

    const empty = { points: [] as TrendPoint[], polyline: '', yTicks: [] as AxisTick[], xLabels: [] as XLabel[] };
    if (n === 0) {
      return empty;
    }

    const plotWidth = this.chartWidth - this.paddingLeft - this.paddingRight;
    const plotHeight = this.chartHeight - this.paddingTop - this.paddingBottom;

    const crsValues = ordered.map(d => d.minimumCRS);
    const min = Math.min(...crsValues);
    const max = Math.max(...crsValues);

    const yFor = (crs: number) => this.paddingTop + (max === min ? plotHeight / 2 : ((max - crs) / (max - min)) * plotHeight);

    const points: TrendPoint[] = ordered.map((d, i) => ({
      drawNumber: d.drawNumber,
      date: d.date,
      minimumCrs: d.minimumCRS,
      category: d.category,
      x: this.paddingLeft + (n === 1 ? plotWidth / 2 : (i / (n - 1)) * plotWidth),
      y: yFor(d.minimumCRS),
    }));

    const yTicks: AxisTick[] = Array.from({ length: Y_TICK_COUNT }, (_, i) => {
      const value = Math.round(min + ((max - min) * i) / (Y_TICK_COUNT - 1));
      return { value, y: yFor(value) };
    });

    const labelCount = Math.min(X_LABEL_COUNT, n);
    const xLabels: XLabel[] = Array.from({ length: labelCount }, (_, i) => {
      const index = Math.round((i * (n - 1)) / (labelCount - 1 || 1));
      return { date: ordered[index].date, x: points[index].x };
    });

    return { points, polyline: points.map(p => `${p.x},${p.y}`).join(' '), yTicks, xLabels };
  });

  constructor(private drawService: ExpressEntryDrawService) {}

  ngOnInit(): void {
    this.loadDraws();
    this.loadCategories();
    this.loadYears();
  }

  loadYears(): void {
    this.drawService.getYears().subscribe({
      next: (years) => this.years.set(years),
      error: (err) => console.error('Erro ao buscar anos', err)
    });
  }

  loadCategories(): void {
    this.isLoadingCategories.set(true);
    this.drawService.getCategories().subscribe({
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

  onFilterChange(): void {
    this.loadDraws();
  }

  loadDraws(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.drawService.getDraws(1, ALL_DRAWS_PAGE_SIZE, this.selectedYear() ?? undefined, this.selectedCategory() ?? undefined).subscribe({
      next: (result: PagedResult<ExpressEntryDraw>) => {
        this.draws.set(result.items);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Erro ao buscar draws', err);
        this.errorMessage.set('Não foi possível carregar os dados. Tente novamente.');
        this.isLoading.set(false);
      }
    });
  }
}
