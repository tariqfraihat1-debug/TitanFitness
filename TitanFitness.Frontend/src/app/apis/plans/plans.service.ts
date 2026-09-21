import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_URL } from '../../config/api.config';
import { ApiResponse } from '../common/api-response.dto';
import { PagedResponse } from '../common/paged-response.dto';
import { AccessScopeFilter } from './access-scope-filter';
import { PlanDetailsDto } from './plan-details.dto';
import { PlanListItemDto } from './plan-list-item.dto';
import { PlanRequestDto } from './plan-request.dto';

@Injectable({
  providedIn: 'root'
})
export class PlansService {
  private readonly http = inject(HttpClient);
  private readonly url = `${API_URL}/plans`;

  getPlans(
    accessScope: AccessScopeFilter | null,
    search: string,
    page: number
  ): Observable<ApiResponse<PagedResponse<PlanListItemDto>>> {
    let params = new HttpParams()
      .set('page', page);

    if (accessScope !== null)
      params = params.set('accessScope', accessScope);

    if (search.trim())
      params = params.set('search', search.trim());

    return this.http.get<ApiResponse<PagedResponse<PlanListItemDto>>>(
      this.url,
      { params }
    );
  }

  getPlanById(planId: number): Observable<ApiResponse<PlanDetailsDto>> {
    return this.http.get<ApiResponse<PlanDetailsDto>>(
      `${this.url}/${planId}`
    );
  }

  createPlan(plan: PlanRequestDto): Observable<ApiResponse<number>> {
    return this.http.post<ApiResponse<number>>(
      this.url,
      plan
    );
  }

  updatePlan(
    planId: number,
    plan: PlanRequestDto
  ): Observable<void> {
    return this.http.put<void>(
      `${this.url}/${planId}`,
      plan
    );
  }
}
