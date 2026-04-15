import { createFeatureSelector, createSelector } from '@ngrx/store';
import { UsersState } from './users.reducer';

export const selectUsersState = createFeatureSelector<UsersState>('users');

export const selectAllUsers = createSelector(selectUsersState, s => s.users);
export const selectAllInvites = createSelector(selectUsersState, s => s.invites);
export const selectUsersLoading = createSelector(selectUsersState, s => s.loading);
export const selectUsersError = createSelector(selectUsersState, s => s.error);
export const selectGeneratedToken = createSelector(selectUsersState, s => s.generatedToken);

export const selectPendingInvites = createSelector(
  selectAllInvites,
  invites => invites.filter(i => i.isPending)
);
