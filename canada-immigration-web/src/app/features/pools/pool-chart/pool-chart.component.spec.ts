import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { PoolChartComponent } from './pool-chart.component';
import { ExpressEntryDrawService } from '../../../core/services/express-entry-draw.service';
import { PoolDistribution } from '../../../models/express-entry-draw.model';

describe('PoolChartComponent', () => {
  const mockPool: PoolDistribution = {
    drawNumber: 441,
    drawDate: '2026-09-04',
    totalCandidates: 226673,
    ranges: [
      { key: 'dd2', range: '501-600', value: 19542 },
      { key: 'dd17', range: '0-300', value: 7735 },
    ],
  };

  async function setup(fakeService: Partial<ExpressEntryDrawService>) {
    await TestBed.configureTestingModule({
      imports: [PoolChartComponent],
      providers: [{ provide: ExpressEntryDrawService, useValue: fakeService }],
    }).compileComponents();

    const fixture = TestBed.createComponent(PoolChartComponent);
    return fixture;
  }

  it('deve criar o componente', async () => {
    const fixture = await setup({ getPoolDistribution: () => of(mockPool) });
    expect(fixture.componentInstance).toBeTruthy();
  });

  describe('quando a chamada tem sucesso', () => {
    it('deve preencher o signal pool e desligar o loading', async () => {
      const fixture = await setup({ getPoolDistribution: () => of(mockPool) });
      const component = fixture.componentInstance;

      fixture.detectChanges();
      
      expect(component.pool()).toEqual(mockPool);
      expect(component.isLoading()).toBe(false);
      expect(component.errorMessage()).toBeNull();
    });

    it('deve renderizar os cards de métrica e as barras no DOM', async () => {
      const fixture = await setup({ getPoolDistribution: () => of(mockPool) });
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;

      expect(compiled.textContent).toContain('#441');
      expect(compiled.querySelectorAll('rect.bar-rect').length).toBe(2);
      expect(compiled.querySelectorAll('text.bar-label').length).toBe(2);
    });

    it('deve calcular a largura das barras proporcionalmente ao maior valor', async () => {
      const fixture = await setup({ getPoolDistribution: () => of(mockPool) });
      const component = fixture.componentInstance;

      fixture.detectChanges();

      const bars = component.chartBars();
      expect(bars[0].width).toBe(component.maxBarAreaWidth);
      expect(bars[1].width).toBeCloseTo((7735 / 19542) * component.maxBarAreaWidth, 5);
    });
  });

  describe('quando a chamada falha', () => {
    it('deve setar errorMessage e desligar o loading, sem preencher pool', async () => {
      const fixture = await setup({
        getPoolDistribution: () => throwError(() => new Error('falha de rede')),
      });
      const component = fixture.componentInstance;

      fixture.detectChanges();

      expect(component.pool()).toBeNull();
      expect(component.isLoading()).toBe(false);
      expect(component.errorMessage()).toBe('Não foi possível carregar os dados do pool. Tente novamente.');
    });

    it('deve exibir a mensagem de erro no DOM', async () => {
      const fixture = await setup({
        getPoolDistribution: () => throwError(() => new Error('falha de rede')),
      });
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.querySelector('.error')?.textContent).toContain('Não foi possível carregar');
    });
  });
});