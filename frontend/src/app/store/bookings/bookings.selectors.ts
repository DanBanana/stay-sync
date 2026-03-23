import { createFeatureSelector, createSelector } from '@ngrx/store';
import { BookingsState } from './bookings.reducer';

export const selectBookingsState = createFeatureSelector<BookingsState>('bookings');
export const selectAllBookings = createSelector(selectBookingsState, s => s.bookings);
export const selectBookingsLoading = createSelector(selectBookingsState, s => s.loading);
export const selectBookingsError = createSelector(selectBookingsState, s => s.error);
