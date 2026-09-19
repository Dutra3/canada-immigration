import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { SubscribeComponent } from './subscribe.component';
import { SubscriptionService } from '../../core/services/subscription.service';
import { ALL_CATEGORIES_VALUE } from '../../models/subscription.model';

describe('SubscribeComponent', () => {
  async function setup(fakeService: Partial<SubscriptionService>) {
    await TestBed.configureTestingModule({
      imports: [SubscribeComponent],
      providers: [{
        provide: SubscriptionService,
        useValue: { getCategories: () => of([]), ...fakeService },
      }],
    }).compileComponents();

    return TestBed.createComponent(SubscribeComponent);
  }

  function buildSelectEvent(values: string[], selectedValues: string[]): Event {
    const options = values.map(value => ({ value, selected: selectedValues.includes(value) }));
    const select = {
      options,
      selectedOptions: options.filter(o => o.selected),
    };
    return { target: select } as unknown as Event;
  }

  it('deve criar o componente', async () => {
    const fixture = await setup({});
    expect(fixture.componentInstance).toBeTruthy();
  });

  describe('ao carregar (ngOnInit)', () => {
    it('deve chamar getCategories e preencher categories', async () => {
      const getCategoriesSpy = vi.fn().mockReturnValue(of(['CEC', 'PNP']));

      const fixture = await setup({ getCategories: getCategoriesSpy });
      fixture.detectChanges();

      expect(getCategoriesSpy).toHaveBeenCalled();
      expect(fixture.componentInstance.categories()).toEqual(['CEC', 'PNP']);
      expect(fixture.componentInstance.isLoadingCategories()).toBe(false);
    });

    it('deve parar o loading mesmo se getCategories falhar', async () => {
      const fixture = await setup({
        getCategories: () => throwError(() => new Error('falha de rede')),
      });
      fixture.detectChanges();

      expect(fixture.componentInstance.isLoadingCategories()).toBe(false);
      expect(fixture.componentInstance.categories()).toEqual([]);
    });
  });

  describe('onCategoriesChange', () => {
    it('deve guardar as categorias selecionadas', async () => {
      const fixture = await setup({});
      const component = fixture.componentInstance;

      component.onCategoriesChange(buildSelectEvent(['CEC', 'PNP', 'French'], ['CEC', 'PNP']));

      expect(component.selectedCategories()).toEqual(['CEC', 'PNP']);
    });

    it('deve colapsar para ALL quando ALL está entre as selecionadas', async () => {
      const fixture = await setup({});
      const component = fixture.componentInstance;

      const event = buildSelectEvent([ALL_CATEGORIES_VALUE, 'CEC', 'PNP'], [ALL_CATEGORIES_VALUE, 'CEC']);
      component.onCategoriesChange(event);

      expect(component.selectedCategories()).toEqual([ALL_CATEGORIES_VALUE]);

      const options = (event.target as unknown as { options: { value: string; selected: boolean }[] }).options;
      expect(options.filter(o => o.selected).map(o => o.value)).toEqual([ALL_CATEGORIES_VALUE]);
    });
  });

  describe('subscribe', () => {
    it('deve exigir e-mail', async () => {
      const subscribeSpy = vi.fn().mockReturnValue(of({ message: 'ok' }));
      const fixture = await setup({ subscribe: subscribeSpy });
      const component = fixture.componentInstance;

      component.selectedCategories.set(['CEC']);
      component.subscribe();

      expect(component.errorMessage()).toBe('Informe um e-mail.');
      expect(subscribeSpy).not.toHaveBeenCalled();
    });

    it('deve exigir ao menos uma categoria', async () => {
      const subscribeSpy = vi.fn().mockReturnValue(of({ message: 'ok' }));
      const fixture = await setup({ subscribe: subscribeSpy });
      const component = fixture.componentInstance;

      component.email.set('user@example.com');
      component.subscribe();

      expect(component.errorMessage()).toBe('Selecione ao menos uma categoria (ou "Todas/Qualquer").');
      expect(subscribeSpy).not.toHaveBeenCalled();
    });

    it('deve chamar o service com e-mail trimado e categorias, exibindo sucesso', async () => {
      const subscribeSpy = vi.fn().mockReturnValue(of({ message: 'Inscrição criada com sucesso.' }));
      const fixture = await setup({ subscribe: subscribeSpy });
      const component = fixture.componentInstance;

      component.email.set('  user@example.com  ');
      component.selectedCategories.set(['CEC', 'PNP']);
      component.subscribe();

      expect(subscribeSpy).toHaveBeenCalledWith({
        email: 'user@example.com',
        categories: ['CEC', 'PNP'],
      });
      expect(component.successMessage()).toBe('Inscrição criada com sucesso.');
      expect(component.errorMessage()).toBeNull();
      expect(component.isLoading()).toBe(false);
    });

    it('deve exibir a mensagem de erro da API quando o subscribe falhar', async () => {
      const fixture = await setup({
        subscribe: () => throwError(() => ({ error: 'Informe um e-mail válido.' })),
      });
      const component = fixture.componentInstance;

      component.email.set('user@example.com');
      component.selectedCategories.set(['CEC']);
      component.subscribe();

      expect(component.errorMessage()).toBe('Informe um e-mail válido.');
      expect(component.successMessage()).toBeNull();
      expect(component.isLoading()).toBe(false);
    });

    it('deve exibir mensagem genérica quando o erro não tem corpo', async () => {
      const fixture = await setup({
        subscribe: () => throwError(() => new Error('falha de rede')),
      });
      const component = fixture.componentInstance;

      component.email.set('user@example.com');
      component.selectedCategories.set(['CEC']);
      component.subscribe();

      expect(component.errorMessage()).toBe('Não foi possível concluir a inscrição. Tente novamente.');
    });
  });
});
