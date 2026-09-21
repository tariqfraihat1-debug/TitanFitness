import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_URL } from '../../config/api.config';
import { ApiResponse } from '../common/api-response.dto';
import { PagedResponse } from '../common/paged-response.dto';
import { ClassSessionDaySummaryDto } from './class-session-day-summary.dto';
import { ClassSessionListItemDto } from './class-session-list-item.dto';
import { CreateClassSessionDto } from './create-class-session.dto';
import { ClassSessionDetailsDto } from './class-session-details.dto';

@Injectable({
  providedIn: 'root'
})
export class ClassSessionsService {
  private readonly http = inject(HttpClient);
  private readonly url = `${API_URL}/class-sessions`;

  getClassSessions(
    branchId: number | null,
    date: string,
    page: number
  ): Observable<ApiResponse<PagedResponse<ClassSessionListItemDto>>> {
    let params = new HttpParams()
      .set('date', date)
      .set('page', page);

    if (branchId !== null)
      params = params.set('branchId', branchId);

    return this.http.get<ApiResponse<PagedResponse<ClassSessionListItemDto>>>(
      this.url,
      { params }
    );
  }

  getDaySummary(
    branchId: number | null,
    date: string
  ): Observable<ApiResponse<ClassSessionDaySummaryDto>> {
    let params = new HttpParams()
      .set('date', date);

    if (branchId !== null)
      params = params.set('branchId', branchId);

    return this.http.get<ApiResponse<ClassSessionDaySummaryDto>>(
      `${this.url}/day-summary`,
      { params }
    );
  }
  createClassSession(session: CreateClassSessionDto): Observable<ApiResponse<number>> {
    return this.http.post<ApiResponse<number>>(this.url, session);
  }

  getClassSessionById(sessionId: number): Observable<ApiResponse<ClassSessionDetailsDto>> {
    return this.http.get<ApiResponse<ClassSessionDetailsDto>>(
      `${this.url}/${sessionId}`
    );
  }
}
