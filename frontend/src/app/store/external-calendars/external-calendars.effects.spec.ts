import { TestBed } from '@angular/core/testing';
import { provideMockActions } from '@ngrx/effects/testing';
import { Observable, of, throwError } from 'rxjs';
import { Action } from '@ngrx/store';
import { ExternalCalendarsEffects } from './external-calendars.effects';
import { ExternalCalendarsActions } from './external-calendars.actions';
import { ExternalCalendarService } from '../../core/services/external-calendar.service';
import { ExternalCalendar } from '../../core/models/external-calendar.model';

const mockCalendar: ExternalCalendar = {
  id: 'c1', roomId: 'r1', platform: 'Airbnb', icsUrl: 'https://airbnb.com/ical/test.ics', lastSyncedAt: null, createdAt: ''
};

describe('ExternalCalendarsEffects', () => {
  let actions$: Observable<Action>;
  let effects: ExternalCalendarsEffects;
  let calendarService: jasmine.SpyObj<ExternalCalendarService>;

  beforeEach(() => {
    calendarService = jasmine.createSpyObj('ExternalCalendarService', ['getByRoom', 'create', 'delete']);

    TestBed.configureTestingModule({
      providers: [
        ExternalCalendarsEffects,
        provideMockActions(() => actions$),
        { provide: ExternalCalendarService, useValue: calendarService },
      ],
    });

    effects = TestBed.inject(ExternalCalendarsEffects);
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
});
