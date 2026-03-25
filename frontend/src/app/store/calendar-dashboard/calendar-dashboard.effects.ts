import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Store } from '@ngrx/store';
import { EMPTY, catchError, map, of, switchMap, tap, withLatestFrom } from 'rxjs';
import { BookingService } from '../../core/services/booking.service';
import { CalendarDashboardActions } from './calendar-dashboard.actions';
import { selectCalendarDashboardState, selectCalendarPropertyId, selectNeedsLoad } from './calendar-dashboard.selectors';

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

  createBooking$ = createEffect(() =>
    this.actions$.pipe(
      ofType(CalendarDashboardActions.createBooking),
      switchMap(({ roomId, checkIn, checkOut, guestName }) =>
        this.bookingService.createManual({ roomId, checkIn, checkOut, guestName }).pipe(
          map(() => CalendarDashboardActions.createBookingSuccess()),
          catchError(err => of(CalendarDashboardActions.createBookingFailure({ error: err.message })))
        )
      )
    )
  );

  updateBooking$ = createEffect(() =>
    this.actions$.pipe(
      ofType(CalendarDashboardActions.updateBooking),
      switchMap(({ id, checkIn, checkOut, guestName }) =>
        this.bookingService.updateManual(id, { checkIn, checkOut, guestName }).pipe(
          map(() => CalendarDashboardActions.updateBookingSuccess()),
          catchError(err => of(CalendarDashboardActions.updateBookingFailure({ error: err.message })))
        )
      )
    )
  );

  deleteBooking$ = createEffect(() =>
    this.actions$.pipe(
      ofType(CalendarDashboardActions.deleteBooking),
      switchMap(({ id }) =>
        this.bookingService.deleteManual(id).pipe(
          map(() => CalendarDashboardActions.deleteBookingSuccess()),
          catchError(err => of(CalendarDashboardActions.deleteBookingFailure({ error: err.message })))
        )
      )
    )
  );

  reloadAfterMutation$ = createEffect(() =>
    this.actions$.pipe(
      ofType(
        CalendarDashboardActions.createBookingSuccess,
        CalendarDashboardActions.updateBookingSuccess,
        CalendarDashboardActions.deleteBookingSuccess,
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
        CalendarDashboardActions.loadCalendarFailure,
        CalendarDashboardActions.createBookingFailure,
        CalendarDashboardActions.updateBookingFailure,
        CalendarDashboardActions.deleteBookingFailure,
      ),
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
