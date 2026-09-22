import { Pipe, PipeTransform } from '@angular/core';

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

    return CATEGORY_LABELS[category] ?? category;
  }
}
