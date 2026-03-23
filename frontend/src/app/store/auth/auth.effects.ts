import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, of, switchMap, tap } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';
import { AuthUser } from '../../core/models/auth.model';
import * as AuthActions from './auth.actions';
import { jwtDecode } from 'jwt-decode';

interface JwtPayload {
  sub: string;
  email: string;
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role': string;
  property_manager_id?: string;
}

@Injectable()
export class AuthEffects {
  login$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.login),
      switchMap(({ request }) =>
        this.authService.login(request).pipe(
          map(result => {
            const decoded = jwtDecode<JwtPayload>(result.token);
            const user: AuthUser = {
              id: decoded.sub,
              email: decoded.email,
              role: decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] as AuthUser['role'],
              propertyManagerId: decoded.property_manager_id ?? null
            };
            return AuthActions.loginSuccess({ token: result.token, user });
          }),
          catchError(err => {
            const message = err?.error?.title ?? 'Login failed. Please check your credentials.';
            return of(AuthActions.loginFailure({ error: message }));
          })
        )
      )
    )
  );

  loginSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(AuthActions.loginSuccess),
        tap(() => this.router.navigate(['/properties']))
      ),
    { dispatch: false }
  );

  logout$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(AuthActions.logout),
        tap(() => this.router.navigate(['/login']))
      ),
    { dispatch: false }
  );

  constructor(
    private actions$: Actions,
    private authService: AuthService,
    private router: Router
  ) {}
}
