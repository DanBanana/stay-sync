import { createFeatureSelector, createSelector } from '@ngrx/store';
import { ExternalCalendarsState } from './external-calendars.reducer';

export const selectExternalCalendarsState = createFeatureSelector<ExternalCalendarsState>('externalCalendars');
export const selectAllCalendars = createSelector(selectExternalCalendarsState, s => s.calendars);
export const selectCalendarsLoading = createSelector(selectExternalCalendarsState, s => s.loading);
export const selectCalendarsError = createSelector(selectExternalCalendarsState, s => s.error);
export const selectSyncingId = createSelector(selectExternalCalendarsState, s => s.syncingId);
