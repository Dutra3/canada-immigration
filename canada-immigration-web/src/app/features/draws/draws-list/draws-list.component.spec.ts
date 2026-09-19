import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { DrawsListComponent } from './draws-list.component';
import { ExpressEntryDrawService } from '../../../core/services/express-entry-draw.service';
import { ExpressEntryDraw, PagedResult } from '../../../models/express-entry-draw.model';

describe('DrawsListComponent', () => {
  const mockDraw: ExpressEntryDraw = {
    drawNumber: 441,
    date: '2026-09-04',
    invitationsIssued: 3500,
    minimumCRS: 486,
    category: 'CEC',
    year: '2026',
  };

  const mockPagedResult: PagedResult<ExpressEntryDraw> = {
    items: [mockDraw],
    page: 1,
    pageSize: 20,
    totalCount: 1,
    totalPages: 1,
  };

  async function setup(fakeService: Partial<ExpressEntryDrawService>) {
    await TestBed.configureTestingModule({
      imports: [DrawsListComponent],
      providers: [{
        provide: ExpressEntryDrawService,
        useValue: { getCategories: () => of([]), getYears: () => of([]), ...fakeService },
      }],
    }).compileComponents();

    const fixture = TestBed.createComponent(DrawsListComponent);
    return fixture;
  }

  it('deve criar o componente', async () => {
    const fixture = await setup({
      getDraws: () => of(mockPagedResult),
      getLatestDraw: () => of(mockDraw),
    });
    expect(fixture.componentInstance).toBeTruthy();
  });

  describe('ao carregar (ngOnInit)', () => {
    it('deve chamar getDraws e getLatestDraw de forma independente', async () => {
      const getDrawsSpy = vi.fn().mockReturnValue(of(mockPagedResult));
      const getLatestDrawSpy = vi.fn().mockReturnValue(of(mockDraw));

      const fixture = await setup({ getDraws: getDrawsSpy, getLatestDraw: getLatestDrawSpy });
      fixture.detectChanges();

      expect(getDrawsSpy).toHaveBeenCalledWith(1, 20, undefined, undefined);
      expect(getLatestDrawSpy).toHaveBeenCalled();
    });

    it('deve preencher draws e latestDraw quando ambas as chamadas têm sucesso', async () => {
      const fixture = await setup({
        getDraws: () => of(mockPagedResult),
        getLatestDraw: () => of(mockDraw),
      });
      const component = fixture.componentInstance;

      fixture.detectChanges();

      expect(component.draws()).toEqual([mockDraw]);
      expect(component.latestDraw()).toEqual(mockDraw);
      expect(component.currentPage()).toBe(1);
      expect(component.totalPages()).toBe(1);
      expect(component.isLoading()).toBe(false);
    });

    it('deve manter os cards de métrica mesmo se getDraws falhar, contanto que getLatestDraw funcione', async () => {
      const fixture = await setup({
        getDraws: () => throwError(() => new Error('falha de rede')),
        getLatestDraw: () => of(mockDraw),
      });
      const component = fixture.componentInstance;

      fixture.detectChanges();

      expect(component.errorMessage()).toBe('Não foi possível carregar os dados. Tente novamente.');
      expect(component.latestDraw()).toEqual(mockDraw);
    });

    it('deve manter a tabela funcionando mesmo se getLatestDraw falhar', async () => {
      const fixture = await setup({
        getDraws: () => of(mockPagedResult),
        getLatestDraw: () => throwError(() => new Error('falha de rede')),
      });
      const component = fixture.componentInstance;

      fixture.detectChanges();

      expect(component.draws()).toEqual([mockDraw]);
      expect(component.latestDraw()).toBeNull();
      expect(component.errorMessage()).toBeNull();
    });
  });

  describe('renderização no DOM', () => {
    it('deve exibir os cards de métrica com os dados de latestDraw, não da tabela paginada', async () => {
      const outroDrawDaPagina: ExpressEntryDraw = {
        drawNumber: 200,
        date: '2024-01-01',
        invitationsIssued: 1000,
        minimumCRS: 500,
        category: null,
        year: '2024',
      };

      const fixture = await setup({
        getDraws: () => of({ ...mockPagedResult, items: [outroDrawDaPagina] }),
        getLatestDraw: () => of(mockDraw), // #441
      });
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.textContent).toContain('#441');
      expect(compiled.textContent).not.toContain('#200');
    });
  });

  describe('filtros', () => {
    it('deve chamar getCategories no init para preencher o select de categorias', async () => {
      const getCategoriesSpy = vi.fn().mockReturnValue(of(['CEC', 'PNP']));

      const fixture = await setup({
        getDraws: () => of(mockPagedResult),
        getLatestDraw: () => of(mockDraw),
        getCategories: getCategoriesSpy,
      });
      fixture.detectChanges();

      expect(getCategoriesSpy).toHaveBeenCalled();
      expect(fixture.componentInstance.categories()).toEqual(['CEC', 'PNP']);
    });

    it('deve chamar getYears no init para preencher o select de anos', async () => {
      const getYearsSpy = vi.fn().mockReturnValue(of([2026, 2025]));

      const fixture = await setup({
        getDraws: () => of(mockPagedResult),
        getLatestDraw: () => of(mockDraw),
        getYears: getYearsSpy,
      });
      fixture.detectChanges();

      expect(getYearsSpy).toHaveBeenCalled();
      expect(fixture.componentInstance.years()).toEqual([2026, 2025]);
    });

    it('onFilterChange deve recarregar a página 1 com os filtros selecionados', async () => {
      const getDrawsSpy = vi.fn().mockReturnValue(of(mockPagedResult));

      const fixture = await setup({ getDraws: getDrawsSpy, getLatestDraw: () => of(mockDraw) });
      fixture.detectChanges();
      getDrawsSpy.mockClear();

      const component = fixture.componentInstance;

      component.selectedYear.set(2025);
      component.onFilterChange();
      expect(getDrawsSpy).toHaveBeenCalledWith(1, 20, 2025, undefined);

      component.selectedCategory.set('CEC');
      component.onFilterChange();
      expect(getDrawsSpy).toHaveBeenCalledWith(1, 20, 2025, 'CEC');
    });

    it('paginação deve manter os filtros aplicados', async () => {
      const getDrawsSpy = vi.fn().mockReturnValue(of({ ...mockPagedResult, totalPages: 3 }));

      const fixture = await setup({ getDraws: getDrawsSpy, getLatestDraw: () => of(mockDraw) });
      fixture.detectChanges();
      getDrawsSpy.mockClear();

      const component = fixture.componentInstance;
      component.selectedYear.set(2024);
      component.selectedCategory.set('PNP');
      component.goToNextPage();

      expect(getDrawsSpy).toHaveBeenCalledWith(2, 20, 2024, 'PNP');
    });
  });

  describe('paginação', () => {
    it('goToNextPage deve chamar getDraws com a página seguinte', async () => {
      const getDrawsSpy = vi.fn().mockReturnValue(
        of({ ...mockPagedResult, page: 1, totalPages: 3 })
      );

      const fixture = await setup({ getDraws: getDrawsSpy, getLatestDraw: () => of(mockDraw) });
      fixture.detectChanges();

      fixture.componentInstance.goToNextPage();

      expect(getDrawsSpy).toHaveBeenCalledWith(2, 20, undefined, undefined);
    });

    it('goToPreviousPage não deve chamar getDraws quando já está na página 1', async () => {
      const getDrawsSpy = vi.fn().mockReturnValue(of(mockPagedResult));

      const fixture = await setup({ getDraws: getDrawsSpy, getLatestDraw: () => of(mockDraw) });
      fixture.detectChanges();

      getDrawsSpy.mockClear();

      fixture.componentInstance.goToPreviousPage();

      expect(getDrawsSpy).not.toHaveBeenCalled();
    });
  });
});