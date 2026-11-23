import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { SubscriptionService } from '../../core/services/subscription.service';

type UnsubscribeState = 'loading' | 'success' | 'error';

@Component({
  selector: 'app-unsubscribe',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './unsubscribe.component.html',
  styleUrl: './unsubscribe.component.scss'
})
export class UnsubscribeComponent implements OnInit {
  state = signal<UnsubscribeState>('loading');

  constructor(
    private route: ActivatedRoute,
    private subscriptionService: SubscriptionService
  ) {}

  ngOnInit(): void {
    const token = this.route.snapshot.queryParamMap.get('token');

    if (!token) {
      this.state.set('error');
      return;
    }

    this.subscriptionService.unsubscribe(token).subscribe({
      next: () => this.state.set('success'),
      error: (err) => {
        console.error('Erro ao descadastrar', err);
        this.state.set('error');
      }
    });
  }
}
