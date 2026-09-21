import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_URL } from '../../config/api.config';
import { ApiResponse } from '../common/api-response.dto';
import { PagedResponse } from '../common/paged-response.dto';
import { CreateTrainerDto } from './create-trainer.dto';
import { TrainerDetailsDto } from './trainer-details.dto';
import { TrainerListItemDto } from './trainer-list-item.dto';
import { TrainerLookupItemDto } from './trainer-lookup-item.dto';
import { UpdateTrainerDto } from './update-trainer.dto';

@Injectable({
  providedIn: 'root'
})
export class TrainersService {
  private readonly http = inject(HttpClient);
  private readonly url = `${API_URL}/trainers`;

  getTrainers(
    branchId: number | null,
    search: string,
    page: number
  ): Observable<ApiResponse<PagedResponse<TrainerListItemDto>>> {
    let params = new HttpParams()
      .set('page', page);

    if (branchId !== null)
      params = params.set('branchId', branchId);

    if (search.trim())
      params = params.set('search', search.trim());

    return this.http.get<ApiResponse<PagedResponse<TrainerListItemDto>>>(
      this.url,
      { params }
    );
  }

  getAvailableTrainers(branchId: number): Observable<ApiResponse<TrainerLookupItemDto[]>> {
    const params = new HttpParams()
      .set('branchId', branchId);

    return this.http.get<ApiResponse<TrainerLookupItemDto[]>>(
      `${this.url}/available`,
      { params }
    );
  }

  getTrainerById(trainerId: number): Observable<ApiResponse<TrainerDetailsDto>> {
    return this.http.get<ApiResponse<TrainerDetailsDto>>(
      `${this.url}/${trainerId}`
    );
  }

  createTrainer(trainer: CreateTrainerDto): Observable<ApiResponse<number>> {
    return this.http.post<ApiResponse<number>>(
      this.url,
      trainer
    );
  }

  updateTrainer(
    trainerId: number,
    trainer: UpdateTrainerDto
  ): Observable<void> {
    return this.http.put<void>(
      `${this.url}/${trainerId}`,
      trainer
    );
  }
}
