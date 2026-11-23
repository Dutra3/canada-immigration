import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CategoryLabelPipe } from '../../../core/pipes/category-label.pipe';
import { ExpressEntryDrawService } from '../../../core/services/express-entry-draw.service';
import { ExpressEntryDraw } from '../../../models/express-entry-draw.model';
import { Observable, concatMap, of } from 'rxjs';

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

const PAGE_SIZE = 200;
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

    const empty = { points: [] as TrendPoint[], polyline: '', area: '', yTicks: [] as AxisTick[], xLabels: [] as XLabel[] };
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

    const baselineY = this.chartHeight - this.paddingBottom;
    const area = `${points.map(p => `${p.x},${p.y}`).join(' ')} ${points[n - 1].x},${baselineY} ${points[0].x},${baselineY}`;

    return { points, polyline: points.map(p => `${p.x},${p.y}`).join(' '), area, yTicks, xLabels };
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

    const year = this.selectedYear() ?? undefined;
    const category = this.selectedCategory() ?? undefined;

    this.fetchAllDraws(year, category).subscribe({
      next: (draws) => {
        this.draws.set(draws);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Erro ao buscar draws', err);
        this.errorMessage.set('Não foi possível carregar os dados. Tente novamente.');
        this.isLoading.set(false);
      }
    });
  }

  private fetchAllDraws(
    year: number | undefined,
    category: string | undefined,
    page = 1,
    accumulated: ExpressEntryDraw[] = []
  ): Observable<ExpressEntryDraw[]> {
    return this.drawService.getDraws(page, PAGE_SIZE, year, category).pipe(
      concatMap((result) => {
        const all = [...accumulated, ...result.items];
        const hasMore = all.length < result.totalCount && result.items.length === PAGE_SIZE;
        return hasMore
          ? this.fetchAllDraws(year, category, page + 1, all)
          : of(all);
      })
    );
  }
}
