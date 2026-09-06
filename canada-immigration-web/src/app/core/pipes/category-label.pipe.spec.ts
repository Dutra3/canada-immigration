import { CategoryLabelPipe } from './category-label.pipe';

describe('CategoryLabelPipe', () => {
  let pipe: CategoryLabelPipe;

  beforeEach(() => {
    pipe = new CategoryLabelPipe();
  });

  it('deve criar a instância do pipe', () => {
    expect(pipe).toBeTruthy();
  });

  it('deve retornar "Geral" quando a categoria é null', () => {
    expect(pipe.transform(null)).toBe('Geral');
  });

  it('deve mapear códigos conhecidos para o nome oficial do IRCC', () => {
    expect(pipe.transform('CEC')).toBe('Canadian Experience Class');
    expect(pipe.transform('PNP')).toBe('Provincial Nominee Program');
    expect(pipe.transform('French')).toBe('French-Language Proficiency');
    expect(pipe.transform('Healthcare')).toBe('Healthcare and Social Services');
    expect(pipe.transform('STEM')).toBe('STEM Occupations');
    expect(pipe.transform('Transport')).toBe('Transport Occupations');
  });

  it('deve retornar o valor original quando a categoria não está no mapa', () => {
    const valorDesconhecido = 'French-Language proficiency 2026-Version 2';
    expect(pipe.transform(valorDesconhecido)).toBe(valorDesconhecido);
  });

  it('deve ser case-sensitive (não mapeia variações de maiúsculas/minúsculas)', () => {
    expect(pipe.transform('cec')).toBe('cec');
  });
});