import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError, timeout, TimeoutError } from 'rxjs';

const REQUEST_TIMEOUT_MS = 20_000;

function extractServerMessage(error: unknown): string | null {
  if (!(error instanceof HttpErrorResponse)) {
    return null;
  }

  const body: unknown = error.error;

  if (typeof body === 'string' && body.trim().length > 0) {
    return body;
  }

  if (body && typeof body === 'object') {
    const problem = body as { detail?: unknown; title?: unknown; errors?: Record<string, string[]> };
    if (typeof problem.detail === 'string' && problem.detail) {
      return problem.detail;
    }
    if (problem.errors) {
      const first = Object.values(problem.errors).flat()[0];
      if (first) {
        return first;
      }
    }
    if (typeof problem.title === 'string' && problem.title) {
      return problem.title;
    }
  }

  return null;
}

function toFriendlyError(err: unknown): HttpErrorResponse {
  if (err instanceof HttpErrorResponse) {
    const serverMessage = extractServerMessage(err);

    let message: string;
    if (err.status === 0) {
      message = 'Não foi possível conectar ao servidor. Verifique sua conexão e tente novamente.';
    } else if (err.status === 429) {
      message = 'Muitas tentativas em pouco tempo. Aguarde um instante e tente novamente.';
    } else if (err.status >= 500) {
      message = 'O servidor encontrou um erro inesperado. Tente novamente mais tarde.';
    } else {
      message = serverMessage ?? 'Requisição inválida. Verifique os dados e tente novamente.';
    }

    return new HttpErrorResponse({
      error: message,
      headers: err.headers,
      status: err.status,
      statusText: err.statusText,
      url: err.url ?? undefined
    });
  }

  if (err instanceof TimeoutError) {
    return new HttpErrorResponse({
      error: 'O servidor demorou demais para responder. Tente novamente.',
      status: 0,
      statusText: 'Timeout'
    });
  }

  return new HttpErrorResponse({
    error: 'Não foi possível concluir a operação. Tente novamente.',
    status: 0,
    statusText: 'UnknownError'
  });
}

export const apiInterceptor: HttpInterceptorFn = (req, next) =>
  next(req).pipe(
    timeout(REQUEST_TIMEOUT_MS),
    catchError((err) => throwError(() => toFriendlyError(err)))
  );
