import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { CrsCalculatorComponent } from './crs-calculator.component';
import { CrsScoreService } from '../../core/services/crs-score.service';
import { CrsScoreRequest, CrsScoreResult, InvitationAnalysis } from '../../models/crs-score.model';

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

  const mockAnalysis: InvitationAnalysis = {
    score: 489,
    categories: [
      { category: '', latestDrawNumber: 11, latestDrawDate: '2025-07-01', latestCutoff: 500, wouldBeInvited: false, eligibility: 'Eligible' },
      { category: 'French', latestDrawNumber: 30, latestDrawDate: '2025-06-20', latestCutoff: 430, wouldBeInvited: true, eligibility: 'Eligible' },
      { category: 'STEM', latestDrawNumber: 50, latestDrawDate: '2025-04-01', latestCutoff: 480, wouldBeInvited: true, eligibility: 'Unknown' },
      { category: 'PNP', latestDrawNumber: 40, latestDrawDate: '2025-06-25', latestCutoff: 720, wouldBeInvited: false, eligibility: 'NotEligible' }
    ],
    pool: {
      range: '451-500',
      candidatesInRange: 500,
      candidatesBelow: 1000,
      totalCandidates: 2000,
      percentBelow: 50
    }
  };

  function fakeService(overrides: Partial<CrsScoreService> = {}): Partial<CrsScoreService> {
    return {
      calculate: () => of(mockResult),
      analyzeInvitation: () => of(mockAnalysis),
      ...overrides
    };
  }

  async function setup(service: Partial<CrsScoreService> = {}) {
    await TestBed.configureTestingModule({
      imports: [CrsCalculatorComponent],
      providers: [{ provide: CrsScoreService, useValue: fakeService(service) }]
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

  describe('análise de convite', () => {
    it('deve chamar analyzeInvitation com a nota e os sinais do formulário', async () => {
      const analyzeSpy = vi.fn().mockReturnValue(of(mockAnalysis));
      const fixture = await setup({ analyzeInvitation: analyzeSpy });
      const component = fixture.componentInstance;

      component.canadianWorkYears.set(2);
      component.hasProvincialNomination.set(true);
      component.calculate();

      expect(analyzeSpy).toHaveBeenCalledWith(489, 2, false, true);
      expect(component.invitation()).toEqual(mockAnalysis);
    });

    it('deve derivar proficiência em francês da primeira língua quando firstLanguageIsFrench', async () => {
      const analyzeSpy = vi.fn().mockReturnValue(of(mockAnalysis));
      const fixture = await setup({ analyzeInvitation: analyzeSpy });
      const component = fixture.componentInstance;

      component.firstLanguageIsFrench.set(true);
      component.updateAbility('first', 'listening', 9);
      component.calculate();

      expect(analyzeSpy).toHaveBeenCalledWith(489, 0, true, false);
    });

    it('deve derivar proficiência em francês da segunda língua quando primeira é inglês', async () => {
      const analyzeSpy = vi.fn().mockReturnValue(of(mockAnalysis));
      const fixture = await setup({ analyzeInvitation: analyzeSpy });
      const component = fixture.componentInstance;

      component.hasSecondLanguage.set(true); // padrão CLB 7 em tudo
      component.calculate();

      expect(analyzeSpy).toHaveBeenCalledWith(489, 0, true, false);
    });

    it('deve renderizar posição no pool e vereditos por categoria', async () => {
      const fixture = await setup();
      fixture.componentInstance.calculate();
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const invitation = compiled.querySelector('.invitation');

      expect(invitation?.textContent).toContain('Você seria convidado?');
      expect(invitation?.textContent).toContain('451-500');
      expect(invitation?.textContent).toContain('50%');
      expect(invitation?.textContent).toContain('Convidado');
      expect(invitation?.textContent).toContain('Depende da ocupação');
    });

    it('deve ordenar categorias: elegíveis, depois unknown, depois não elegíveis', async () => {
      const fixture = await setup();
      const component = fixture.componentInstance;

      component.calculate();

      const order = component.sortedCategories().map(c => c.eligibility);
      expect(order).toEqual(['Eligible', 'Eligible', 'Unknown', 'NotEligible']);
    });

    it('não deve quebrar quando a análise falha — nota continua visível', async () => {
      const fixture = await setup({ analyzeInvitation: () => throwError(() => new Error('falha')) });
      const component = fixture.componentInstance;

      component.calculate();
      fixture.detectChanges();

      expect(component.result()).toEqual(mockResult);
      expect(component.invitation()).toBeNull();
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.querySelector('.score')?.textContent).toContain('489');
      expect(compiled.querySelector('.invitation')).toBeNull();
    });
  });
});
