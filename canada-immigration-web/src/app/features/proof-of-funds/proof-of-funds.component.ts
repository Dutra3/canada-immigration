import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProofOfFundsService } from '../../core/services/proof-of-funds.service';
import { ProofOfFundsResult } from '../../models/proof-of-funds.model';

@Component({
  selector: 'app-proof-of-funds',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './proof-of-funds.component.html',
  styleUrl: './proof-of-funds.component.scss'
})
export class ProofOfFundsComponent {
  familySize = signal<number>(1);
  result = signal<ProofOfFundsResult | null>(null);
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);

  constructor(private proofOfFundsService: ProofOfFundsService) {}

  calculate(): void {
    if (this.familySize() < 1) {
      this.errorMessage.set('Informe um número de familiares maior que zero.');
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.result.set(null);

    this.proofOfFundsService.calculate(this.familySize()).subscribe({
      next: (res) => {
        this.result.set(res);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Erro ao calcular proof of funds', err);
        this.errorMessage.set('Não foi possível calcular. Tente novamente.');
        this.isLoading.set(false);
      }
    });
  }
}
