import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CrsScoreService } from '../../core/services/crs-score.service';
import {
  CanadianEducationLevel,
  CrsScoreRequest,
  CrsScoreResult,
  EducationLevel,
  LanguageAbilities
} from '../../models/crs-score.model';

interface SelectOption<T> {
  value: T;
  label: string;
}

@Component({
  selector: 'app-crs-calculator',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './crs-calculator.component.html',
  styleUrl: './crs-calculator.component.scss'
})
export class CrsCalculatorComponent {
  readonly abilities: (keyof LanguageAbilities)[] = ['listening', 'reading', 'writing', 'speaking'];

  readonly abilityLabels: Record<keyof LanguageAbilities, string> = {
    listening: 'Compreensão oral',
    reading: 'Leitura',
    writing: 'Escrita',
    speaking: 'Expressão oral'
  };

  readonly clbOptions: SelectOption<number>[] = [
    { value: 0, label: 'Sem teste / abaixo de CLB 4' },
    { value: 4, label: 'CLB 4' },
    { value: 5, label: 'CLB 5' },
    { value: 6, label: 'CLB 6' },
    { value: 7, label: 'CLB 7' },
    { value: 8, label: 'CLB 8' },
    { value: 9, label: 'CLB 9' },
    { value: 10, label: 'CLB 10' }
  ];

  readonly educationOptions: SelectOption<EducationLevel>[] = [
    { value: 'LessThanSecondary', label: 'Menos que ensino médio' },
    { value: 'Secondary', label: 'Ensino médio' },
    { value: 'OneYearPostSecondary', label: 'Programa pós-secundário de 1 ano' },
    { value: 'TwoYearPostSecondary', label: 'Programa pós-secundário de 2+ anos' },
    { value: 'BachelorsOrThreeYear', label: 'Bacharelado / programa de 3+ anos' },
    { value: 'TwoOrMoreCredentials', label: 'Dois ou mais diplomas (um de 3+ anos)' },
    { value: 'MastersOrProfessional', label: 'Mestrado ou grau profissional' },
    { value: 'Doctoral', label: 'Doutorado (PhD)' }
  ];

  readonly canadianEducationOptions: SelectOption<CanadianEducationLevel>[] = [
    { value: 'None', label: 'Nenhum' },
    { value: 'OneOrTwoYears', label: '1 ou 2 anos no Canadá' },
    { value: 'ThreeYearsOrMore', label: '3+ anos no Canadá / mestrado ou doutorado no Canadá' }
  ];

  readonly workYearOptions: SelectOption<number>[] = [
    { value: 0, label: 'Nenhuma' },
    { value: 1, label: '1 ano' },
    { value: 2, label: '2 anos' },
    { value: 3, label: '3 anos' },
    { value: 4, label: '4 anos' },
    { value: 5, label: '5+ anos' }
  ];

  age = signal<number>(30);
  education = signal<EducationLevel>('BachelorsOrThreeYear');
  firstLanguageIsFrench = signal(false);
  firstLanguage = signal<LanguageAbilities>({ listening: 9, reading: 9, writing: 9, speaking: 9 });
  hasSecondLanguage = signal(false);
  secondLanguage = signal<LanguageAbilities>({ listening: 7, reading: 7, writing: 7, speaking: 7 });
  canadianWorkYears = signal<number>(0);
  foreignWorkYears = signal<number>(0);
  hasCertificateOfQualification = signal(false);
  hasProvincialNomination = signal(false);
  hasSiblingInCanada = signal(false);
  canadianEducation = signal<CanadianEducationLevel>('None');

  hasSpouse = signal(false);
  spouseEducation = signal<EducationLevel>('Secondary');
  spouseFirstLanguage = signal<LanguageAbilities>({ listening: 7, reading: 7, writing: 7, speaking: 7 });
  spouseCanadianWorkYears = signal<number>(0);

  result = signal<CrsScoreResult | null>(null);
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);

  constructor(private crsScoreService: CrsScoreService) {}

  updateAbility(target: 'first' | 'second' | 'spouse', ability: keyof LanguageAbilities, value: number): void {
    const current = target === 'first'
      ? this.firstLanguage()
      : target === 'second' ? this.secondLanguage() : this.spouseFirstLanguage();
    const updated = { ...current, [ability]: value };
    if (target === 'first') {
      this.firstLanguage.set(updated);
    } else if (target === 'second') {
      this.secondLanguage.set(updated);
    } else {
      this.spouseFirstLanguage.set(updated);
    }
  }

  calculate(): void {
    if (this.age() < 0) {
      this.errorMessage.set('Informe uma idade válida.');
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.result.set(null);

    const request: CrsScoreRequest = {
      age: this.age(),
      education: this.education(),
      firstLanguageIsFrench: this.firstLanguageIsFrench(),
      firstLanguage: this.firstLanguage(),
      secondLanguage: this.hasSecondLanguage() ? this.secondLanguage() : undefined,
      canadianWorkYears: this.canadianWorkYears(),
      foreignWorkYears: this.foreignWorkYears(),
      hasCertificateOfQualification: this.hasCertificateOfQualification(),
      hasProvincialNomination: this.hasProvincialNomination(),
      hasSiblingInCanada: this.hasSiblingInCanada(),
      canadianEducation: this.canadianEducation(),
      spouse: this.hasSpouse()
        ? {
            education: this.spouseEducation(),
            firstLanguage: this.spouseFirstLanguage(),
            canadianWorkYears: this.spouseCanadianWorkYears()
          }
        : undefined
    };

    this.crsScoreService.calculate(request).subscribe({
      next: (res) => {
        this.result.set(res);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Erro ao calcular CRS', err);
        this.errorMessage.set('Não foi possível calcular. Tente novamente.');
        this.isLoading.set(false);
      }
    });
  }
}
