import { HttpErrorResponse } from '@angular/common/http';

export function getApiErrorMessage(
  error: unknown,
  fallback = 'Something went wrong.'
): string {
  if (error instanceof Error &&
    !(error instanceof HttpErrorResponse) &&
    error.message.trim())
    return error.message;

  if (!(error instanceof HttpErrorResponse))
    return fallback;

  if (error.status === 0)
    return 'Cannot connect to the server.';

  const body = error.error;

  if (Array.isArray(body?.errors) && body.errors.length > 0)
    return body.errors.join(' ');

  if (body?.errors && typeof body.errors === 'object') {
    const messages = Object.values(body.errors)
      .flatMap(value => Array.isArray(value) ? value : [])
      .filter(
        (value): value is string =>
          typeof value === 'string'
      );

    if (messages.length > 0)
      return messages.join(' ');
  }

  if (
    typeof body?.message === 'string' &&
    body.message.trim()
  )
    return body.message;

  if (
    typeof body?.title === 'string' &&
    body.title.trim()
  )
    return body.title;

  return fallback;
}
