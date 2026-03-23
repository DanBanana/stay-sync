import { createReducer, on } from '@ngrx/store';
import { Booking } from '../../core/models/booking.model';
import * as AuthActions from '../auth/auth.actions';
import { BookingsActions } from './bookings.actions';

export interface BookingsState {
  bookings: Booking[];
  loading: boolean;
  error: string | null;
}

export const initialState: BookingsState = { bookings: [], loading: false, error: null };

export const bookingsReducer = createReducer(
  initialState,
  on(BookingsActions.loadBookingsByRoom, BookingsActions.loadBookingsByProperty,
    state => ({ ...state, loading: true, error: null })),
  on(BookingsActions.loadBookingsSuccess, (state, { bookings }) => ({ ...state, bookings, loading: false })),
  on(BookingsActions.loadBookingsFailure, (state, { error }) => ({ ...state, error, loading: false })),
  on(AuthActions.logout, () => initialState),
);
