import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';
import { getApiErrorMessage } from '../apis/common/api-error.util';

export const httpInterceptor: HttpInterceptorFn = (req, next) =>
  next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      const message = getApiErrorMessage(error);

      return throwError(() =>
        new Error(message)
      );
    })
  );
