import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { SubscriptionRequest, SubscriptionResponse } from '../../models/subscription.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class SubscriptionService {
  private readonly subscriptionsUrl = `${environment.apiBaseUrl}/Subscriptions`;
  private readonly drawsUrl = `${environment.apiBaseUrl}/ExpressEntryDraws`;

  constructor(private http: HttpClient) {}

  getCategories(): Observable<string[]> {
    return this.http.get<string[]>(`${this.drawsUrl}/categories`);
  }

  subscribe(request: SubscriptionRequest): Observable<SubscriptionResponse> {
    return this.http.post<SubscriptionResponse>(this.subscriptionsUrl, request);
  }
}
