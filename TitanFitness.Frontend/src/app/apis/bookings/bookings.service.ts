import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_URL } from '../../config/api.config';
import { ApiResponse } from '../common/api-response.dto';
import { CreateBookingDto } from './create-booking.dto';

@Injectable({
  providedIn: 'root'
})
export class BookingsService {
  private readonly http = inject(HttpClient);
  private readonly url = `${API_URL}/bookings`;

  createBooking(booking: CreateBookingDto): Observable<ApiResponse<number>> {
    return this.http.post<ApiResponse<number>>(this.url, booking);
  }
}
