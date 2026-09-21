import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_URL } from '../../config/api.config';
import { ApiResponse } from '../common/api-response.dto';
import { ActiveMembersDto } from './active-members.dto';
import { CheckInsTodayDto } from './check-ins-today.dto';
import { UpcomingClassDto } from './upcoming-class.dto';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private readonly http = inject(HttpClient);
  private readonly url = `${API_URL}/dashboard`;

  getCheckInsToday(): Observable<ApiResponse<CheckInsTodayDto>> {
    return this.http.get<ApiResponse<CheckInsTodayDto>>(`${this.url}/check-ins-today`);
  }

  getActiveMembers(): Observable<ApiResponse<ActiveMembersDto>> {
    return this.http.get<ApiResponse<ActiveMembersDto>>(`${this.url}/active-members`);
  }

  getUpcomingClasses(): Observable<ApiResponse<UpcomingClassDto[]>> {
    return this.http.get<ApiResponse<UpcomingClassDto[]>>(`${this.url}/upcoming-classes`);
  }
}
