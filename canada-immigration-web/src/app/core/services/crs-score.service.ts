import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CrsScoreRequest, CrsScoreResult, InvitationAnalysis } from '../../models/crs-score.model';
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

  analyzeInvitation(
    score: number,
    canadianWorkYears: number,
    hasFrenchProficiency: boolean,
    hasProvincialNomination: boolean
  ): Observable<InvitationAnalysis> {
    const params = new HttpParams()
      .set('score', score)
      .set('canadianWorkYears', canadianWorkYears)
      .set('hasFrenchProficiency', hasFrenchProficiency)
      .set('hasProvincialNomination', hasProvincialNomination);
    return this.http.get<InvitationAnalysis>(`${this.baseUrl}/invitation`, { params });
  }
}
