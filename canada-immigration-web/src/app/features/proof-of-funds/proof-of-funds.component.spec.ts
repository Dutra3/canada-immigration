import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { ProofOfFundsComponent } from './proof-of-funds.component';
import { ProofOfFundsService } from '../../core/services/proof-of-funds.service';
import { ProofOfFundsResult } from '../../models/proof-of-funds.model';

describe('ProofOfFundsComponent', () => {
  const mockResult: ProofOfFundsResult = {
    familySize: 4,
    requiredFundsCad: 28362,
  };

  async function setup(fakeService: Partial<ProofOfFundsService>) {
    await TestBed.configureTestingModule({
      imports: [ProofOfFundsComponent],
      providers: [{ provide: ProofOfFundsService, useValue: fakeService }],
    }).compileComponents();

    const fixture = TestBed.createComponent(ProofOfFundsComponent);
    return fixture;
  }

  it('deve criar o componente com familySize padrão de 1', async () => {
    const fixture = await setup({ calculate: () => of(mockResult) });
    expect(fixture.componentInstance.familySize()).toBe(1);
  });

  it('não deve chamar o service ao carregar (só reage ao clique)', async () => {
    const calculateSpy = vi.fn().mockReturnValue(of(mockResult));
    const fixture = await setup({ calculate: calculateSpy });

    fixture.detectChanges();

    expect(calculateSpy).not.toHaveBeenCalled();
  });

  describe('validação local (sem chamar o service)', () => {
    it('deve bloquear familySize igual a zero e mostrar mensagem de erro', async () => {
      const calculateSpy = vi.fn().mockReturnValue(of(mockResult));
      const fixture = await setup({ calculate: calculateSpy });
      const component = fixture.componentInstance;

      component.familySize.set(0);
      component.calculate();

      expect(calculateSpy).not.toHaveBeenCalled();
      expect(component.errorMessage()).toBe('Informe um número de familiares maior que zero.');
    });

    it('deve bloquear familySize negativo', async () => {
      const calculateSpy = vi.fn().mockReturnValue(of(mockResult));
      const fixture = await setup({ calculate: calculateSpy });
      const component = fixture.componentInstance;

      component.familySize.set(-1);
      component.calculate();

      expect(calculateSpy).not.toHaveBeenCalled();
    });
  });

  describe('cálculo com sucesso', () => {
    it('deve chamar calculate com o familySize atual e preencher result', async () => {
      const calculateSpy = vi.fn().mockReturnValue(of(mockResult));
      const fixture = await setup({ calculate: calculateSpy });
      const component = fixture.componentInstance;

      component.familySize.set(4);
      component.calculate();

      expect(calculateSpy).toHaveBeenCalledWith(4);
      expect(component.result()).toEqual(mockResult);
      expect(component.isLoading()).toBe(false);
      expect(component.errorMessage()).toBeNull();
    });

    it('deve limpar result e errorMessage anteriores antes de uma nova chamada', async () => {
      const calculateSpy = vi.fn().mockReturnValue(of(mockResult));
      const fixture = await setup({ calculate: calculateSpy });
      const component = fixture.componentInstance;

      component.errorMessage.set('erro antigo simulado');
      component.result.set({ familySize: 1, requiredFundsCad: 15263 });

      component.familySize.set(4);
      component.calculate();

      expect(component.errorMessage()).toBeNull();
      expect(component.result()).toEqual(mockResult);
    });

    it('deve renderizar o valor calculado no DOM', async () => {
      const fixture = await setup({ calculate: () => of(mockResult) });
      fixture.componentInstance.familySize.set(4);
      fixture.componentInstance.calculate();
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.querySelector('.result')?.textContent).toContain('4');
    });
  });

  describe('cálculo com falha', () => {
    it('deve setar errorMessage e desligar loading quando o service falha', async () => {
      const fixture = await setup({
        calculate: () => throwError(() => new Error('falha de rede')),
      });
      const component = fixture.componentInstance;

      component.familySize.set(4);
      component.calculate();

      expect(component.result()).toBeNull();
      expect(component.isLoading()).toBe(false);
      expect(component.errorMessage()).toBe('Não foi possível calcular. Tente novamente.');
    });
  });
});