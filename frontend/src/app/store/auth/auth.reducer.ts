import { createReducer, on } from '@ngrx/store';
import { AuthUser } from '../../core/models/auth.model';
import * as AuthActions from './auth.actions';

export interface AuthState {
  token: string | null;
  user: AuthUser | null;
  loading: boolean;
  error: string | null;
}

export const initialState: AuthState = {
  token: null,
  user: null,
  loading: false,
  error: null
};

export const authReducer = createReducer(
  initialState,
  on(AuthActions.login, state => ({ ...state, loading: true, error: null })),
  on(AuthActions.loginSuccess, (state, { token, user }) => ({
    ...state,
    token,
    user,
    loading: false,
    error: null
  })),
  on(AuthActions.loginFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error
  })),
  on(AuthActions.logout, () => initialState)
);
