import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_URL } from '../../config/api.config';
import { ApiResponse } from '../common/api-response.dto';
import { CheckInDetailsDto } from './check-in-details.dto';
import { CreateCheckInDto } from './create-check-in.dto';

@Injectable({
  providedIn: 'root'
})
export class CheckInsService {
  private readonly http = inject(HttpClient);
  private readonly url = `${API_URL}/check-ins`;

  createCheckIn(checkIn: CreateCheckInDto): Observable<ApiResponse<CheckInDetailsDto>> {
    return this.http.post<ApiResponse<CheckInDetailsDto>>(
      this.url,
      checkIn
    );
  }
}
