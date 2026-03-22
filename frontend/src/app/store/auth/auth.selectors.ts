import { createFeatureSelector, createSelector } from '@ngrx/store';
import { AuthState } from './auth.reducer';

export const selectAuthState = createFeatureSelector<AuthState>('auth');

export const selectToken = createSelector(selectAuthState, s => s.token);
export const selectUser = createSelector(selectAuthState, s => s.user);
export const selectIsAuthenticated = createSelector(selectAuthState, s => !!s.token);
export const selectRole = createSelector(selectAuthState, s => s.user?.role ?? null);
export const selectAuthLoading = createSelector(selectAuthState, s => s.loading);
export const selectAuthError = createSelector(selectAuthState, s => s.error);
