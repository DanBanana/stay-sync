import { createReducer, on } from '@ngrx/store';
import { ExternalCalendar } from '../../core/models/external-calendar.model';
import * as AuthActions from '../auth/auth.actions';
import { ExternalCalendarsActions } from './external-calendars.actions';

export interface ExternalCalendarsState {
  calendars: ExternalCalendar[];
  loading: boolean;
  error: string | null;
}

export const initialState: ExternalCalendarsState = { calendars: [], loading: false, error: null };

export const externalCalendarsReducer = createReducer(
  initialState,
  on(ExternalCalendarsActions.loadCalendars, state => ({ ...state, loading: true, error: null })),
  on(ExternalCalendarsActions.loadCalendarsSuccess, (state, { calendars }) => ({ ...state, calendars, loading: false })),
  on(ExternalCalendarsActions.loadCalendarsFailure, (state, { error }) => ({ ...state, error, loading: false })),
  on(ExternalCalendarsActions.createCalendarSuccess, (state, { calendar }) => ({
    ...state, calendars: [...state.calendars, calendar]
  })),
  on(ExternalCalendarsActions.deleteCalendarSuccess, (state, { id }) => ({
    ...state, calendars: state.calendars.filter(c => c.id !== id)
  })),
  on(AuthActions.logout, () => initialState),
);
