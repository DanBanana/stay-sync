import { TestBed } from '@angular/core/testing';
import { provideMockActions } from '@ngrx/effects/testing';
import { provideMockStore, MockStore } from '@ngrx/store/testing';
import { Observable, of, throwError } from 'rxjs';
import { Action } from '@ngrx/store';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ExternalCalendarsEffects } from './external-calendars.effects';
import { ExternalCalendarsActions } from './external-calendars.actions';
import { ExternalCalendarService } from '../../core/services/external-calendar.service';
import { ExternalCalendar, SyncCalendarResult } from '../../core/models/external-calendar.model';
import { CalendarDashboardActions } from '../calendar-dashboard/calendar-dashboard.actions';
import { selectCalendarPropertyId } from '../calendar-dashboard/calendar-dashboard.selectors';

const mockCalendar: ExternalCalendar = {
  id: 'c1', roomId: 'r1', platform: 'Airbnb', icsUrl: 'https://airbnb.com/ical/test.ics',
  lastSyncedAt: null, lastSyncStatus: null, lastSyncErrorMessage: null, createdAt: ''
};

const mockSyncResult: SyncCalendarResult = { inserted: 3, updated: 1 };

describe('ExternalCalendarsEffects', () => {
  let actions$: Observable<Action>;
  let effects: ExternalCalendarsEffects;
  let calendarService: jasmine.SpyObj<ExternalCalendarService>;
  let store: MockStore;
  let snackBar: jasmine.SpyObj<MatSnackBar>;

  beforeEach(() => {
    calendarService = jasmine.createSpyObj('ExternalCalendarService', ['getByRoom', 'create', 'delete', 'sync']);
    snackBar = jasmine.createSpyObj('MatSnackBar', ['open']);

    TestBed.configureTestingModule({
      providers: [
        ExternalCalendarsEffects,
        provideMockActions(() => actions$),
        provideMockStore({ initialState: { calendarDashboard: { selectedPropertyId: null } } }),
        { provide: ExternalCalendarService, useValue: calendarService },
        { provide: MatSnackBar, useValue: snackBar },
      ],
    });

    effects = TestBed.inject(ExternalCalendarsEffects);
    store = TestBed.inject(MockStore);
  });

  it('loadCalendars$ dispatches success with calendars', done => {
    calendarService.getByRoom.and.returnValue(of([mockCalendar]));
    actions$ = of(ExternalCalendarsActions.loadCalendars({ roomId: 'r1' }));

    effects.loadCalendars$.subscribe(action => {
      expect(action).toEqual(ExternalCalendarsActions.loadCalendarsSuccess({ calendars: [mockCalendar] }));
      done();
    });
  });

  it('loadCalendars$ dispatches failure on error', done => {
    calendarService.getByRoom.and.returnValue(throwError(() => new Error('Network error')));
    actions$ = of(ExternalCalendarsActions.loadCalendars({ roomId: 'r1' }));

    effects.loadCalendars$.subscribe(action => {
      expect(action.type).toEqual(ExternalCalendarsActions.loadCalendarsFailure.type);
      done();
    });
  });

  it('createCalendar$ dispatches success with calendar', done => {
    calendarService.create.and.returnValue(of(mockCalendar));
    actions$ = of(ExternalCalendarsActions.createCalendar({ roomId: 'r1', platform: 'Airbnb', icsUrl: 'https://airbnb.com/ical/test.ics' }));

    effects.createCalendar$.subscribe(action => {
      expect(action).toEqual(ExternalCalendarsActions.createCalendarSuccess({ calendar: mockCalendar }));
      done();
    });
  });

  it('syncCalendar$ dispatches syncCalendarSuccess on service success', done => {
    calendarService.sync.and.returnValue(of(mockSyncResult));
    actions$ = of(ExternalCalendarsActions.syncCalendar({ id: 'c1' }));

    effects.syncCalendar$.subscribe(action => {
      expect(action).toEqual(ExternalCalendarsActions.syncCalendarSuccess({ id: 'c1', result: mockSyncResult }));
      done();
    });
  });

  it('syncCalendar$ dispatches syncCalendarFailure with id on service error', done => {
    calendarService.sync.and.returnValue(throwError(() => ({ error: { message: 'ICS fetch failed' } })));
    actions$ = of(ExternalCalendarsActions.syncCalendar({ id: 'c1' }));

    effects.syncCalendar$.subscribe(action => {
      expect(action).toEqual(ExternalCalendarsActions.syncCalendarFailure({ id: 'c1', error: 'ICS fetch failed' }));
      done();
    });
  });

  it('syncSuccess$ dispatches setProperty when propertyId is available', done => {
    store.overrideSelector(selectCalendarPropertyId, 'p1');
    store.refreshState();
    actions$ = of(ExternalCalendarsActions.syncCalendarSuccess({ id: 'c1', result: mockSyncResult }));

    effects.syncSuccess$.subscribe(action => {
      expect(action).toEqual(CalendarDashboardActions.setProperty({ propertyId: 'p1' }));
      done();
    });
  });
});
