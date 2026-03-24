import { TestBed } from '@angular/core/testing';
import { provideMockActions } from '@ngrx/effects/testing';
import { provideMockStore, MockStore } from '@ngrx/store/testing';
import { Observable, of, throwError } from 'rxjs';
import { Action } from '@ngrx/store';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { CalendarDashboardEffects } from './calendar-dashboard.effects';
import { CalendarDashboardActions } from './calendar-dashboard.actions';
import { BookingService } from '../../core/services/booking.service';
import { CalendarBooking } from '../../core/models/booking.model';
import { selectCalendarDashboardState, selectNeedsLoad } from './calendar-dashboard.selectors';
import { initialState } from './calendar-dashboard.reducer';

const mockBooking: CalendarBooking = {
  id: 'b1', roomId: 'r1', roomName: 'Room A',
  platform: 'Airbnb', checkIn: '2026-04-05', checkOut: '2026-04-10', status: 'Confirmed'
};

describe('CalendarDashboardEffects', () => {
  let actions$: Observable<Action>;
  let effects: CalendarDashboardEffects;
  let bookingService: jasmine.SpyObj<BookingService>;
  let store: MockStore;

  beforeEach(() => {
    bookingService = jasmine.createSpyObj('BookingService', ['getForCalendar']);

    TestBed.configureTestingModule({
      imports: [MatSnackBarModule],
      providers: [
        CalendarDashboardEffects,
        provideMockActions(() => actions$),
        provideMockStore({
          initialState: { calendarDashboard: initialState },
        }),
        { provide: BookingService, useValue: bookingService },
      ],
    });

    effects = TestBed.inject(CalendarDashboardEffects);
    store = TestBed.inject(MockStore);
  });

  it('loadCalendar$ dispatches success with bookings', done => {
    bookingService.getForCalendar.and.returnValue(of([mockBooking]));
    actions$ = of(CalendarDashboardActions.loadCalendar({
      propertyId: 'p1', from: '2026-03-24', to: '2026-05-04'
    }));

    effects.loadCalendar$.subscribe(action => {
      expect(action).toEqual(CalendarDashboardActions.loadCalendarSuccess({
        bookings: [mockBooking], from: '2026-03-24', to: '2026-05-04'
      }));
      done();
    });
  });

  it('loadCalendar$ dispatches failure on error', done => {
    bookingService.getForCalendar.and.returnValue(throwError(() => new Error('Network error')));
    actions$ = of(CalendarDashboardActions.loadCalendar({
      propertyId: 'p1', from: '2026-03-24', to: '2026-05-04'
    }));

    effects.loadCalendar$.subscribe(action => {
      expect(action.type).toEqual(CalendarDashboardActions.loadCalendarFailure.type);
      done();
    });
  });

  it('triggerLoad$ dispatches loadCalendar when property is set and range not loaded', done => {
    const stateWithProperty = {
      ...initialState,
      selectedPropertyId: 'p1',
      loadedRange: null,
    };
    store.overrideSelector(selectCalendarDashboardState, stateWithProperty);
    store.overrideSelector(selectNeedsLoad, { needed: true, from: '2026-03-17', to: '2026-04-28' });
    store.refreshState();

    actions$ = of(CalendarDashboardActions.setProperty({ propertyId: 'p1' }));

    effects.triggerLoad$.subscribe(action => {
      expect(action.type).toEqual(CalendarDashboardActions.loadCalendar.type);
      done();
    });
  });
});
