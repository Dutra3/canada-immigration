import { Pipe, PipeTransform } from '@angular/core';

// Mapeia os códigos crus que a API externa devolve para nomes
// legíveis em português/inglês, do jeito que o IRCC nomeia oficialmente
// cada categoria de draw.
const CATEGORY_LABELS: Record<string, string> = {
  'CEC': 'Canadian Experience Class',
  'PNP': 'Provincial Nominee Program',
  'French': 'French-Language Proficiency',
  'Healthcare': 'Healthcare and Social Services',
  'STEM': 'STEM Occupations',
  'Transport': 'Transport Occupations',
};

@Pipe({
  name: 'categoryLabel',
  standalone: true
})
export class CategoryLabelPipe implements PipeTransform {
  transform(category: string | null): string {
    if (!category) {
      return 'Geral';
    }

    // Alguns valores vêm com sufixo de versão, ex: "French-Language proficiency 2026-Version 2"
    // Procura por correspondência exata primeiro; se não achar, devolve o valor original sem alterar.
    return CATEGORY_LABELS[category] ?? category;
  }
}
