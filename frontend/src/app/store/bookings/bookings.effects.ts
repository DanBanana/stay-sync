import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, of, switchMap, tap } from 'rxjs';
import { BookingService } from '../../core/services/booking.service';
import { BookingsActions } from './bookings.actions';

@Injectable()
export class BookingsEffects {
  loadByRoom$ = createEffect(() =>
    this.actions$.pipe(
      ofType(BookingsActions.loadBookingsByRoom),
      switchMap(({ roomId }) =>
        this.bookingService.getByRoom(roomId).pipe(
          map(bookings => BookingsActions.loadBookingsSuccess({ bookings })),
          catchError(err => of(BookingsActions.loadBookingsFailure({ error: err.message })))
        )
      )
    )
  );

  loadByProperty$ = createEffect(() =>
    this.actions$.pipe(
      ofType(BookingsActions.loadBookingsByProperty),
      switchMap(({ propertyId }) =>
        this.bookingService.getByProperty(propertyId).pipe(
          map(bookings => BookingsActions.loadBookingsSuccess({ bookings })),
          catchError(err => of(BookingsActions.loadBookingsFailure({ error: err.message })))
        )
      )
    )
  );

  notifyFailure$ = createEffect(() =>
    this.actions$.pipe(
      ofType(BookingsActions.loadBookingsFailure),
      tap(({ error }) => this.snackBar.open(`Error: ${error}`, 'Dismiss', { duration: 5000 }))
    ),
    { dispatch: false }
  );

  constructor(private actions$: Actions, private bookingService: BookingService, private snackBar: MatSnackBar) {}
}
