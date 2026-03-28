import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Store } from '@ngrx/store';
import { EMPTY, catchError, map, of, switchMap, tap, withLatestFrom } from 'rxjs';
import { ExternalCalendarService } from '../../core/services/external-calendar.service';
import { CalendarDashboardActions } from '../calendar-dashboard/calendar-dashboard.actions';
import { selectCalendarPropertyId } from '../calendar-dashboard/calendar-dashboard.selectors';
import { ExternalCalendarsActions } from './external-calendars.actions';

@Injectable()
export class ExternalCalendarsEffects {
  loadCalendars$ = createEffect(() =>
    this.actions$.pipe(
      ofType(ExternalCalendarsActions.loadCalendars),
      switchMap(({ roomId }) =>
        this.calendarService.getByRoom(roomId).pipe(
          map(calendars => ExternalCalendarsActions.loadCalendarsSuccess({ calendars })),
          catchError(err => of(ExternalCalendarsActions.loadCalendarsFailure({ error: err.message })))
        )
      )
    )
  );

  createCalendar$ = createEffect(() =>
    this.actions$.pipe(
      ofType(ExternalCalendarsActions.createCalendar),
      switchMap(({ roomId, platform, icsUrl }) =>
        this.calendarService.create(roomId, platform, icsUrl).pipe(
          map(calendar => ExternalCalendarsActions.createCalendarSuccess({ calendar })),
          catchError(err => of(ExternalCalendarsActions.createCalendarFailure({ error: err.message })))
        )
      )
    )
  );

  deleteCalendar$ = createEffect(() =>
    this.actions$.pipe(
      ofType(ExternalCalendarsActions.deleteCalendar),
      switchMap(({ id }) =>
        this.calendarService.delete(id).pipe(
          map(() => ExternalCalendarsActions.deleteCalendarSuccess({ id })),
          catchError(err => of(ExternalCalendarsActions.deleteCalendarFailure({ error: err.message })))
        )
      )
    )
  );

  syncCalendar$ = createEffect(() =>
    this.actions$.pipe(
      ofType(ExternalCalendarsActions.syncCalendar),
      switchMap(({ id }) =>
        this.calendarService.sync(id).pipe(
          map(result => ExternalCalendarsActions.syncCalendarSuccess({ id, result })),
          catchError(err => of(ExternalCalendarsActions.syncCalendarFailure({ error: err.error?.message ?? err.message })))
        )
      )
    )
  );

  syncSuccess$ = createEffect(() =>
    this.actions$.pipe(
      ofType(ExternalCalendarsActions.syncCalendarSuccess),
      tap(({ result }) =>
        this.snackBar.open(
          `Sync complete — ${result.inserted} new, ${result.updated} updated`,
          'Dismiss',
          { duration: 4000 }
        )
      ),
      withLatestFrom(this.store.select(selectCalendarPropertyId)),
      switchMap(([_, propertyId]) => {
        if (!propertyId) return EMPTY;
        return of(CalendarDashboardActions.setProperty({ propertyId }));
      })
    )
  );

  notifyFailure$ = createEffect(() =>
    this.actions$.pipe(
      ofType(
        ExternalCalendarsActions.createCalendarFailure,
        ExternalCalendarsActions.deleteCalendarFailure,
        ExternalCalendarsActions.loadCalendarsFailure,
        ExternalCalendarsActions.syncCalendarFailure,
      ),
      tap(({ error }) => this.snackBar.open(`Error: ${error}`, 'Dismiss', { duration: 5000 }))
    ),
    { dispatch: false }
  );

  constructor(
    private actions$: Actions,
    private calendarService: ExternalCalendarService,
    private store: Store,
    private snackBar: MatSnackBar
  ) {}
}
