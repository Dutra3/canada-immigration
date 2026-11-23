import { Pipe, PipeTransform } from '@angular/core';

const CATEGORY_CLASSES: Record<string, string> = {
  'CEC': 'CEC',
  'PNP': 'PNP',
  'French': 'French',
  'Healthcare': 'Healthcare',
  'STEM': 'STEM',
  'Transport': 'Transport',
};

@Pipe({
  name: 'categoryClass',
  standalone: true
})
export class CategoryClassPipe implements PipeTransform {
  transform(category: string | null): string {
    if (!category) {
      return 'General';
    }

    return CATEGORY_CLASSES[category] ?? 'General';
  }
}
