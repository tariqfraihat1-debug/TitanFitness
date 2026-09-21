import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_URL } from '../../config/api.config';
import { ApiResponse } from '../common/api-response.dto';
import { BranchDto } from './branch.dto';

@Injectable({
  providedIn: 'root'
})
export class BranchesService {
  private readonly http = inject(HttpClient);
  private readonly url = `${API_URL}/branches`;

  getBranches(): Observable<ApiResponse<BranchDto[]>> {
    return this.http.get<ApiResponse<BranchDto[]>>(this.url);
  }
}
