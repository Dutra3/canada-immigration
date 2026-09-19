import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CrsScoreRequest, CrsScoreResult } from '../../models/crs-score.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class CrsScoreService {
  private readonly baseUrl = `${environment.apiBaseUrl}/CrsScore`;

  constructor(private http: HttpClient) {}

  calculate(request: CrsScoreRequest): Observable<CrsScoreResult> {
    return this.http.post<CrsScoreResult>(this.baseUrl, request);
  }
}
