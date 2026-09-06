import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ExpressEntryDrawService } from '../../../core/services/express-entry-draw.service';
import { PoolDistribution } from '../../../models/express-entry-draw.model';

interface ChartBar {
  range: string;
  value: number;
  width: number;
  y: number;
}

@Component({
  selector: 'app-pool-chart',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './pool-chart.component.html',
  styleUrl: './pool-chart.component.scss'
})
export class PoolChartComponent implements OnInit {
  pool = signal<PoolDistribution | null>(null);
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);

  readonly labelWidth = 90;
  readonly maxBarAreaWidth = 380;
  readonly valueAreaWidth = 70;
  readonly barHeight = 22;
  readonly barGap = 10;
  readonly chartWidth = this.labelWidth + this.maxBarAreaWidth + this.valueAreaWidth;

  chartBars = computed<ChartBar[]>(() => {
    const ranges = this.pool()?.ranges ?? [];
    const maxValue = Math.max(...ranges.map(r => r.value), 1);

    return ranges.map((r, index) => ({
      range: r.range,
      value: r.value,
      width: (r.value / maxValue) * this.maxBarAreaWidth,
      y: index * (this.barHeight + this.barGap)
    }));
  });

  chartHeight = computed(() => {
    const count = this.pool()?.ranges.length ?? 0;
    return count > 0 ? count * (this.barHeight + this.barGap) : this.barHeight;
  });

  constructor(private drawService: ExpressEntryDrawService) {}

  ngOnInit(): void {
    this.loadPool();
  }

  loadPool(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.drawService.getPoolDistribution().subscribe({
      next: (result) => {
        this.pool.set(result);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Erro ao buscar distribuição do pool', err);
        this.errorMessage.set('Não foi possível carregar os dados do pool. Tente novamente.');
        this.isLoading.set(false);
      }
    });
  }
}