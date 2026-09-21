import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_URL } from '../../config/api.config';
import { ApiResponse } from '../common/api-response.dto';
import { AddFreezeDto } from './add-freeze.dto';
import { ChangeMembershipPlanDto } from './change-membership-plan.dto';
import { MembershipDetailsDto } from './membership-details.dto';
import { PlanLookupItemDto } from './plan-lookup-item.dto';

@Injectable({
  providedIn: 'root'
})
export class MembershipsService {
  private readonly http = inject(HttpClient);
  private readonly url = `${API_URL}/memberships`;

  getMembershipById(membershipId: number): Observable<ApiResponse<MembershipDetailsDto>> {
    return this.http.get<ApiResponse<MembershipDetailsDto>>(
      `${this.url}/${membershipId}`
    );
  }

  getAvailablePlans(membershipId: number): Observable<ApiResponse<PlanLookupItemDto[]>> {
    return this.http.get<ApiResponse<PlanLookupItemDto[]>>(
      `${this.url}/${membershipId}/available-plans`
    );
  }

  renewMembership(membershipId: number): Observable<ApiResponse<number>> {
    return this.http.post<ApiResponse<number>>(
      `${this.url}/${membershipId}/renew`,
      {}
    );
  }

  changePlan(
    membershipId: number,
    request: ChangeMembershipPlanDto
  ): Observable<ApiResponse<number>> {
    return this.http.post<ApiResponse<number>>(
      `${this.url}/${membershipId}/change-plan`,
      request
    );
  }

  addFreeze(
    membershipId: number,
    freeze: AddFreezeDto
  ): Observable<ApiResponse<number>> {
    return this.http.post<ApiResponse<number>>(
      `${this.url}/${membershipId}/freezes`,
      freeze
    );
  }
}
