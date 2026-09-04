import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ProofOfFundsResult } from '../../models/proof-of-funds.model';

@Injectable({
  providedIn: 'root'
})
export class ProofOfFundsService {
  private readonly baseUrl = 'http://localhost:5088/api/ProofOfFunds';

  constructor(private http: HttpClient) {}

  calculate(familySize: number): Observable<ProofOfFundsResult> {
    const params = new HttpParams().set('familySize', familySize.toString());
    return this.http.get<ProofOfFundsResult>(this.baseUrl, { params });
  }
}
