import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { App } from './app';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideRouter([])],
    }).compileComponents();
  });

  it('deve criar o componente', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('deve renderizar as abas de navegação', async () => {
    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();
    const compiled = fixture.nativeElement as HTMLElement;

    const links = compiled.querySelectorAll('nav.tabs a');
    expect(links.length).toBe(3);
    expect(links[0].textContent).toContain('Express Entry Draws');
    expect(links[1].textContent).toContain('Proof of Funds');
    expect(links[2].textContent).toContain('Pool CRS');
  });
});