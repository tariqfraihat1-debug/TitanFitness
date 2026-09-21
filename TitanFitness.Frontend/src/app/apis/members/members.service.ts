import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_URL } from '../../config/api.config';
import { ApiResponse } from '../common/api-response.dto';
import { PagedResponse } from '../common/paged-response.dto';
import { CreateMemberDto } from './create-member.dto';
import { CurrentMembershipDto } from './current-membership.dto';
import { EntryEligibilityDto } from './entry-eligibility.dto';
import { MemberActivityDto } from './member-activity.dto';
import { MemberDetailsDto } from './member-details.dto';
import { MemberListItemDto } from './member-list-item.dto';
import { UpdateMemberDto } from './update-member.dto';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  private readonly http = inject(HttpClient);
  private readonly url = `${API_URL}/members`;

  getMembers(branchId: number | null, search: string, page: number): Observable<ApiResponse<PagedResponse<MemberListItemDto>>> {
    let params = new HttpParams()
      .set('page', page);

    if (branchId !== null)
      params = params.set('branchId', branchId);

    if (search.trim())
      params = params.set('search', search.trim());

    return this.http.get<ApiResponse<PagedResponse<MemberListItemDto>>>(
      this.url,
      { params }
    );
  }

  getMemberById(memberId: number): Observable<ApiResponse<MemberDetailsDto>> {
    return this.http.get<ApiResponse<MemberDetailsDto>>(
      `${this.url}/${memberId}`
    );
  }

  getCurrentMembership(memberId: number): Observable<ApiResponse<CurrentMembershipDto>> {
    return this.http.get<ApiResponse<CurrentMembershipDto>>(
      `${this.url}/${memberId}/current-membership`
    );
  }

  getMemberActivity(memberId: number): Observable<ApiResponse<MemberActivityDto[]>> {
    return this.http.get<ApiResponse<MemberActivityDto[]>>(
      `${this.url}/${memberId}/activity`
    );
  }

  getEntryEligibility(memberId: number, branchId: number): Observable<ApiResponse<EntryEligibilityDto>> {
    const params = new HttpParams()
      .set('branchId', branchId);

    return this.http.get<ApiResponse<EntryEligibilityDto>>(
      `${this.url}/${memberId}/entry-eligibility`,
      { params }
    );
  }

  createMember(member: CreateMemberDto): Observable<ApiResponse<number>> {
    return this.http.post<ApiResponse<number>>(
      this.url,
      member
    );
  }

  updateMember(memberId: number, member: UpdateMemberDto): Observable<void> {
    return this.http.put<void>(
      `${this.url}/${memberId}`,
      member
    );
  }
}
