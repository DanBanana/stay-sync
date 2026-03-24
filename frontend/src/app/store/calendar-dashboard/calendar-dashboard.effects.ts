import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Store } from '@ngrx/store';
import { EMPTY, catchError, map, of, switchMap, tap, withLatestFrom } from 'rxjs';
import { BookingService } from '../../core/services/booking.service';
import { CalendarDashboardActions } from './calendar-dashboard.actions';
import { selectCalendarDashboardState, selectNeedsLoad } from './calendar-dashboard.selectors';

@Injectable()
export class CalendarDashboardEffects {
  triggerLoad$ = createEffect(() =>
    this.actions$.pipe(
      ofType(CalendarDashboardActions.setProperty, CalendarDashboardActions.navigateWindow),
      withLatestFrom(
        this.store.select(selectCalendarDashboardState),
        this.store.select(selectNeedsLoad)
      ),
      switchMap(([_, state, needsLoad]) => {
        if (!state.selectedPropertyId || !needsLoad.needed) return EMPTY;
        return of(CalendarDashboardActions.loadCalendar({
          propertyId: state.selectedPropertyId,
          from: needsLoad.from,
          to: needsLoad.to,
        }));
      })
    )
  );

  loadCalendar$ = createEffect(() =>
    this.actions$.pipe(
      ofType(CalendarDashboardActions.loadCalendar),
      switchMap(({ propertyId, from, to }) =>
        this.bookingService.getForCalendar(propertyId, from, to).pipe(
          map(bookings => CalendarDashboardActions.loadCalendarSuccess({ bookings, from, to })),
          catchError(err => of(CalendarDashboardActions.loadCalendarFailure({ error: err.message })))
        )
      )
    )
  );

  notifyFailure$ = createEffect(() =>
    this.actions$.pipe(
      ofType(CalendarDashboardActions.loadCalendarFailure),
      tap(({ error }) => this.snackBar.open(`Error: ${error}`, 'Dismiss', { duration: 5000 }))
    ),
    { dispatch: false }
  );

  constructor(
    private actions$: Actions,
    private store: Store,
    private bookingService: BookingService,
    private snackBar: MatSnackBar
  ) {}
}
