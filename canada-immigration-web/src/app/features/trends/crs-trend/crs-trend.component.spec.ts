import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { CrsTrendComponent } from './crs-trend.component';
import { ExpressEntryDrawService } from '../../../core/services/express-entry-draw.service';
import { ExpressEntryDraw, PagedResult } from '../../../models/express-entry-draw.model';

describe('CrsTrendComponent', () => {
  const mockDraws: ExpressEntryDraw[] = [
    { drawNumber: 441, date: '2026-09-04', invitationsIssued: 3500, minimumCRS: 486, category: 'CEC', year: '2026' },
    { drawNumber: 440, date: '2026-08-20', invitationsIssued: 2500, minimumCRS: 491, category: 'CEC', year: '2026' },
    { drawNumber: 439, date: '2026-08-05', invitationsIssued: 3000, minimumCRS: 480, category: 'CEC', year: '2026' },
  ];

  const mockPagedResult: PagedResult<ExpressEntryDraw> = {
    items: mockDraws,
    page: 1,
    pageSize: 200,
    totalCount: 3,
    totalPages: 1,
  };

  async function setup(fakeService: Partial<ExpressEntryDrawService>) {
    await TestBed.configureTestingModule({
      imports: [CrsTrendComponent],
      providers: [{
        provide: ExpressEntryDrawService,
        useValue: { getCategories: () => of([]), getYears: () => of([]), ...fakeService },
      }],
    }).compileComponents();

    const fixture = TestBed.createComponent(CrsTrendComponent);
    return fixture;
  }

  it('deve criar o componente', async () => {
    const fixture = await setup({
      getDraws: () => of(mockPagedResult),
    });
    expect(fixture.componentInstance).toBeTruthy();
  });

  describe('ao carregar (ngOnInit)', () => {
    it('deve chamar getDraws sem filtros, getCategories e getYears', async () => {
      const getDrawsSpy = vi.fn().mockReturnValue(of(mockPagedResult));
      const getCategoriesSpy = vi.fn().mockReturnValue(of(['CEC', 'PNP']));
      const getYearsSpy = vi.fn().mockReturnValue(of([2026, 2025]));

      const fixture = await setup({ getDraws: getDrawsSpy, getCategories: getCategoriesSpy, getYears: getYearsSpy });
      fixture.detectChanges();

      expect(getDrawsSpy).toHaveBeenCalledWith(1, 200, undefined, undefined);
      expect(getCategoriesSpy).toHaveBeenCalled();
      expect(getYearsSpy).toHaveBeenCalled();
      expect(fixture.componentInstance.categories()).toEqual(['CEC', 'PNP']);
      expect(fixture.componentInstance.years()).toEqual([2026, 2025]);
    });

    it('deve exibir mensagem de erro quando getDraws falhar', async () => {
      const fixture = await setup({
        getDraws: () => throwError(() => new Error('falha de rede')),
      });
      const component = fixture.componentInstance;

      fixture.detectChanges();

      expect(component.errorMessage()).toBe('Não foi possível carregar os dados. Tente novamente.');
      expect(component.draws()).toEqual([]);
    });
  });

  describe('filtros', () => {
    it('onFilterChange deve recarregar os draws com a categoria selecionada', async () => {
      const getDrawsSpy = vi.fn().mockReturnValue(of(mockPagedResult));

      const fixture = await setup({ getDraws: getDrawsSpy });
      fixture.detectChanges();
      getDrawsSpy.mockClear();

      const component = fixture.componentInstance;
      component.selectedCategory.set('PNP');
      component.onFilterChange();

      expect(getDrawsSpy).toHaveBeenCalledWith(1, 200, undefined, 'PNP');
    });

    it('onFilterChange deve recarregar os draws com o ano selecionado', async () => {
      const getDrawsSpy = vi.fn().mockReturnValue(of(mockPagedResult));

      const fixture = await setup({ getDraws: getDrawsSpy });
      fixture.detectChanges();
      getDrawsSpy.mockClear();

      const component = fixture.componentInstance;
      component.selectedYear.set(2025);
      component.onFilterChange();

      expect(getDrawsSpy).toHaveBeenCalledWith(1, 200, 2025, undefined);
    });

    it('onFilterChange deve combinar ano e categoria', async () => {
      const getDrawsSpy = vi.fn().mockReturnValue(of(mockPagedResult));

      const fixture = await setup({ getDraws: getDrawsSpy });
      fixture.detectChanges();
      getDrawsSpy.mockClear();

      const component = fixture.componentInstance;
      component.selectedYear.set(2024);
      component.selectedCategory.set('CEC');
      component.onFilterChange();

      expect(getDrawsSpy).toHaveBeenCalledWith(1, 200, 2024, 'CEC');
    });
  });

  describe('dados do gráfico', () => {
    it('deve ordenar os pontos por data crescente', async () => {
      const fixture = await setup({
        getDraws: () => of(mockPagedResult),
      });
      fixture.detectChanges();

      const points = fixture.componentInstance.chart().points;

      expect(points.map(p => p.drawNumber)).toEqual([439, 440, 441]);
    });

    it('deve mapear o maior CRS para o topo do gráfico', async () => {
      const fixture = await setup({
        getDraws: () => of(mockPagedResult),
      });
      fixture.detectChanges();

      const component = fixture.componentInstance;
      const points = component.chart().points;
      const highest = points.find(p => p.minimumCrs === 491)!;
      const lowest = points.find(p => p.minimumCrs === 480)!;

      expect(highest.y).toBe(component.paddingTop);
      expect(lowest.y).toBe(component.chartHeight - component.paddingBottom);
    });

    it('deve gerar polyline e ticks com os mesmos pontos', async () => {
      const fixture = await setup({
        getDraws: () => of(mockPagedResult),
      });
      fixture.detectChanges();

      const chart = fixture.componentInstance.chart();

      expect(chart.polyline.split(' ').length).toBe(chart.points.length);
      expect(chart.yTicks.length).toBe(5);
      expect(chart.xLabels.length).toBeLessThanOrEqual(6);
    });

    it('deve retornar gráfico vazio quando não houver draws', async () => {
      const fixture = await setup({
        getDraws: () => of({ ...mockPagedResult, items: [] }),
      });
      fixture.detectChanges();

      expect(fixture.componentInstance.chart().points).toEqual([]);
      expect(fixture.componentInstance.chart().polyline).toBe('');
    });
  });

  describe('paginação', () => {
    it('deve buscar todas as páginas quando totalCount excede o pageSize', async () => {
      const page1Items = Array.from({ length: 200 }, (_, i) => ({
        ...mockDraws[0],
        drawNumber: i + 1,
      }));
      const page1 = { items: page1Items, page: 1, pageSize: 200, totalCount: 201, totalPages: 2 };
      const page2 = { items: [mockDraws[1]], page: 2, pageSize: 200, totalCount: 201, totalPages: 2 };
      const getDrawsSpy = vi.fn().mockImplementation((page: number) => of(page === 1 ? page1 : page2));

      const fixture = await setup({ getDraws: getDrawsSpy });
      fixture.detectChanges();
      await fixture.whenStable();

      expect(getDrawsSpy).toHaveBeenCalledTimes(2);
      expect(fixture.componentInstance.draws().length).toBe(201);
    });
  });
});
