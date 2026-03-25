import { createReducer, on } from '@ngrx/store';
import { CalendarBooking } from '../../core/models/booking.model';
import * as AuthActions from '../auth/auth.actions';
import { CalendarDashboardActions } from './calendar-dashboard.actions';

export interface CalendarDashboardState {
  selectedPropertyId: string | null;
  windowStart: string;
  bookings: CalendarBooking[];
  loadedRange: { from: string; to: string } | null;
  loading: boolean;
  saving: boolean;
  error: string | null;
}

function defaultWindowStart(): string {
  const d = new Date();
  d.setDate(d.getDate() - 7);
  return d.toISOString().slice(0, 10);
}

function addDays(isoDate: string, days: number): string {
  const d = new Date(isoDate);
  d.setDate(d.getDate() + days);
  return d.toISOString().slice(0, 10);
}

function mergeBookings(existing: CalendarBooking[], incoming: CalendarBooking[]): CalendarBooking[] {
  const map = new Map(existing.map(b => [b.id, b]));
  for (const b of incoming) map.set(b.id, b);
  return Array.from(map.values());
}

function expandLoadedRange(
  current: { from: string; to: string } | null,
  from: string,
  to: string
): { from: string; to: string } {
  if (!current) return { from, to };
  return {
    from: from < current.from ? from : current.from,
    to: to > current.to ? to : current.to,
  };
}

export const initialState: CalendarDashboardState = {
  selectedPropertyId: null,
  windowStart: defaultWindowStart(),
  bookings: [],
  loadedRange: null,
  loading: false,
  saving: false,
  error: null,
};

export const calendarDashboardReducer = createReducer(
  initialState,

  on(CalendarDashboardActions.setProperty, (state, { propertyId }) => ({
    ...state,
    selectedPropertyId: propertyId,
    bookings: [],
    loadedRange: null,
    error: null,
  })),

  on(CalendarDashboardActions.navigateWindow, (state, { direction }) => ({
    ...state,
    windowStart: addDays(state.windowStart, direction === 'forward' ? 7 : -7),
  })),

  on(CalendarDashboardActions.loadCalendar, state => ({ ...state, loading: true, error: null })),

  on(CalendarDashboardActions.loadCalendarSuccess, (state, { bookings, from, to }) => ({
    ...state,
    loading: false,
    bookings: mergeBookings(state.bookings, bookings),
    loadedRange: expandLoadedRange(state.loadedRange, from, to),
  })),

  on(CalendarDashboardActions.loadCalendarFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  on(
    CalendarDashboardActions.createBooking,
    CalendarDashboardActions.updateBooking,
    CalendarDashboardActions.deleteBooking,
    state => ({ ...state, saving: true, error: null })
  ),

  on(
    CalendarDashboardActions.createBookingSuccess,
    CalendarDashboardActions.updateBookingSuccess,
    CalendarDashboardActions.deleteBookingSuccess,
    state => ({ ...state, saving: false })
  ),

  on(
    CalendarDashboardActions.createBookingFailure,
    CalendarDashboardActions.updateBookingFailure,
    CalendarDashboardActions.deleteBookingFailure,
    (state, { error }) => ({ ...state, saving: false, error })
  ),

  on(CalendarDashboardActions.reset, () => initialState),
  on(AuthActions.logout, () => initialState),
);
