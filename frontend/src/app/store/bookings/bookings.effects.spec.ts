import { TestBed } from '@angular/core/testing';
import { provideMockActions } from '@ngrx/effects/testing';
import { Observable, of, throwError } from 'rxjs';
import { Action } from '@ngrx/store';
import { BookingsEffects } from './bookings.effects';
import { BookingsActions } from './bookings.actions';
import { BookingService } from '../../core/services/booking.service';
import { Booking } from '../../core/models/booking.model';

const mockBooking: Booking = {
  id: 'b1', roomId: 'r1', externalCalendarId: 'c1', externalUid: 'uid-1',
  guestName: 'Alice', checkIn: '2025-06-01', checkOut: '2025-06-07',
  status: 'Confirmed', rawSummary: null
};

describe('BookingsEffects', () => {
  let actions$: Observable<Action>;
  let effects: BookingsEffects;
  let bookingService: jasmine.SpyObj<BookingService>;

  beforeEach(() => {
    bookingService = jasmine.createSpyObj('BookingService', ['getByRoom', 'getByProperty']);

    TestBed.configureTestingModule({
      providers: [
        BookingsEffects,
        provideMockActions(() => actions$),
        { provide: BookingService, useValue: bookingService },
      ],
    });

    effects = TestBed.inject(BookingsEffects);
  });

  it('loadByRoom$ dispatches success with bookings', done => {
    bookingService.getByRoom.and.returnValue(of([mockBooking]));
    actions$ = of(BookingsActions.loadBookingsByRoom({ roomId: 'r1' }));

    effects.loadByRoom$.subscribe(action => {
      expect(action).toEqual(BookingsActions.loadBookingsSuccess({ bookings: [mockBooking] }));
      done();
    });
  });

  it('loadByRoom$ dispatches failure on error', done => {
    bookingService.getByRoom.and.returnValue(throwError(() => new Error('Network error')));
    actions$ = of(BookingsActions.loadBookingsByRoom({ roomId: 'r1' }));

    effects.loadByRoom$.subscribe(action => {
      expect(action.type).toEqual(BookingsActions.loadBookingsFailure.type);
      done();
    });
  });
});
