import { createFeatureSelector, createSelector } from '@ngrx/store';
import { RoomsState } from './rooms.reducer';

export const selectRoomsState = createFeatureSelector<RoomsState>('rooms');
export const selectAllRooms = createSelector(selectRoomsState, s => s.rooms);
export const selectRoomsLoading = createSelector(selectRoomsState, s => s.loading);
export const selectRoomsError = createSelector(selectRoomsState, s => s.error);
