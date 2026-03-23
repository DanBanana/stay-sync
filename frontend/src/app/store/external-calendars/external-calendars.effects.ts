import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, of, switchMap, tap } from 'rxjs';
import { ExternalCalendarService } from '../../core/services/external-calendar.service';
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

  notifyFailure$ = createEffect(() =>
    this.actions$.pipe(
      ofType(
        ExternalCalendarsActions.createCalendarFailure,
        ExternalCalendarsActions.deleteCalendarFailure,
        ExternalCalendarsActions.loadCalendarsFailure,
      ),
      tap(({ error }) => this.snackBar.open(`Error: ${error}`, 'Dismiss', { duration: 5000 }))
    ),
    { dispatch: false }
  );

  constructor(private actions$: Actions, private calendarService: ExternalCalendarService, private snackBar: MatSnackBar) {}
}
