import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_URL } from '../../config/api.config';
import { ApiResponse } from '../common/api-response.dto';
import { StudioDto } from './studio.dto';

@Injectable({
  providedIn: 'root'
})
export class StudiosService {
  private readonly http = inject(HttpClient);
  private readonly url = `${API_URL}/studios`;

  getStudios(branchId: number): Observable<ApiResponse<StudioDto[]>> {
    const params = new HttpParams().set('branchId', branchId);
    return this.http.get<ApiResponse<StudioDto[]>>(this.url, { params });
  }
}
