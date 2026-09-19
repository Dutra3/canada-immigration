import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { CrsCalculatorComponent } from './crs-calculator.component';
import { CrsScoreService } from '../../core/services/crs-score.service';
import { CrsScoreRequest, CrsScoreResult } from '../../models/crs-score.model';

describe('CrsCalculatorComponent', () => {
  const mockResult: CrsScoreResult = {
    totalScore: 489,
    breakdown: {
      age: 105,
      education: 120,
      firstLanguage: 124,
      secondLanguage: 0,
      canadianWorkExperience: 40,
      spouseFactors: 0,
      skillTransferability: 100,
      additional: 0
    }
  };

  async function setup(fakeService: Partial<CrsScoreService>) {
    await TestBed.configureTestingModule({
      imports: [CrsCalculatorComponent],
      providers: [{ provide: CrsScoreService, useValue: fakeService }]
    }).compileComponents();

    return TestBed.createComponent(CrsCalculatorComponent);
  }

  it('deve criar o componente', async () => {
    const fixture = await setup({ calculate: () => of(mockResult) });
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('deve montar o request sem cônjuge e sem segunda língua por padrão', async () => {
    const calculateSpy = vi.fn().mockReturnValue(of(mockResult));
    const fixture = await setup({ calculate: calculateSpy });
    const component = fixture.componentInstance;

    component.calculate();

    const request = calculateSpy.mock.calls[0][0] as CrsScoreRequest;
    expect(request.spouse).toBeUndefined();
    expect(request.secondLanguage).toBeUndefined();
    expect(request.age).toBe(30);
    expect(request.education).toBe('BachelorsOrThreeYear');
  });

  it('deve incluir cônjuge no request quando hasSpouse está ativo', async () => {
    const calculateSpy = vi.fn().mockReturnValue(of(mockResult));
    const fixture = await setup({ calculate: calculateSpy });
    const component = fixture.componentInstance;

    component.hasSpouse.set(true);
    component.spouseEducation.set('MastersOrProfessional');
    component.calculate();

    const request = calculateSpy.mock.calls[0][0] as CrsScoreRequest;
    expect(request.spouse).toBeDefined();
    expect(request.spouse!.education).toBe('MastersOrProfessional');
    expect(request.spouse!.firstLanguage).toEqual({ listening: 7, reading: 7, writing: 7, speaking: 7 });
  });

  it('deve incluir segunda língua no request quando hasSecondLanguage está ativo', async () => {
    const calculateSpy = vi.fn().mockReturnValue(of(mockResult));
    const fixture = await setup({ calculate: calculateSpy });
    const component = fixture.componentInstance;

    component.hasSecondLanguage.set(true);
    component.calculate();

    const request = calculateSpy.mock.calls[0][0] as CrsScoreRequest;
    expect(request.secondLanguage).toBeDefined();
  });

  it('deve bloquear idade negativa sem chamar o service', async () => {
    const calculateSpy = vi.fn().mockReturnValue(of(mockResult));
    const fixture = await setup({ calculate: calculateSpy });
    const component = fixture.componentInstance;

    component.age.set(-5);
    component.calculate();

    expect(calculateSpy).not.toHaveBeenCalled();
    expect(component.errorMessage()).toBe('Informe uma idade válida.');
  });

  it('deve preencher result e renderizar nota e breakdown no DOM', async () => {
    const fixture = await setup({ calculate: () => of(mockResult) });
    fixture.componentInstance.calculate();
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.score')?.textContent).toContain('489');
    expect(compiled.querySelector('.breakdown')?.textContent).toContain('Transferibilidade');
  });

  it('deve atualizar uma habilidade de idioma via updateAbility', async () => {
    const fixture = await setup({ calculate: () => of(mockResult) });
    const component = fixture.componentInstance;

    component.updateAbility('first', 'listening', 10);

    expect(component.firstLanguage().listening).toBe(10);
    expect(component.firstLanguage().reading).toBe(9);
  });

  it('deve setar errorMessage quando o service falha', async () => {
    const fixture = await setup({ calculate: () => throwError(() => new Error('falha')) });
    const component = fixture.componentInstance;

    component.calculate();

    expect(component.result()).toBeNull();
    expect(component.isLoading()).toBe(false);
    expect(component.errorMessage()).toBe('Não foi possível calcular. Tente novamente.');
  });
});
