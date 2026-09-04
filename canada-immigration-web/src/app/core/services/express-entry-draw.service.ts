import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ExpressEntryDraw, PagedResult } from '../../models/express-entry-draw.model';

@Injectable({
  providedIn: 'root'
})
export class ExpressEntryDrawService {
  private readonly baseUrl = 'http://localhost:5088/api/ExpressEntryDraws';

  constructor(private http: HttpClient) {}

  getDraws(page: number, pageSize: number = 20, year?: number, category?: string): Observable<PagedResult<ExpressEntryDraw>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (year) {
      params = params.set('year', year.toString());
    }
    if (category) {
      params = params.set('category', category);
    }

    return this.http.get<PagedResult<ExpressEntryDraw>>(this.baseUrl, { params });
  }
}
