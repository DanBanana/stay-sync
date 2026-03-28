import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideMockStore, MockStore } from '@ngrx/store/testing';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { RouterTestingModule } from '@angular/router/testing';
import { ActivatedRoute } from '@angular/router';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { ExternalCalendarsPageComponent } from './external-calendars-page.component';
import { ExternalCalendarsActions } from '../../../store/external-calendars/external-calendars.actions';

const initialState = {
  externalCalendars: { calendars: [], loading: false, error: null, syncingId: null }
};

describe('ExternalCalendarsPageComponent', () => {
  let component: ExternalCalendarsPageComponent;
  let fixture: ComponentFixture<ExternalCalendarsPageComponent>;
  let store: MockStore;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ExternalCalendarsPageComponent],
      imports: [RouterTestingModule],
      providers: [
        provideMockStore({ initialState }),
        { provide: MatDialog, useValue: { open: () => ({ afterClosed: () => mockOf(null) }) } },
        { provide: MatSnackBar, useValue: { open: () => {} } },
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: () => 'room-1' } } } },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    store = TestBed.inject(MockStore);
    fixture = TestBed.createComponent(ExternalCalendarsPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should dispatch loadCalendars on init', () => {
    const dispatchSpy = spyOn(store, 'dispatch');
    component.ngOnInit();
    expect(dispatchSpy).toHaveBeenCalledWith(ExternalCalendarsActions.loadCalendars({ roomId: 'room-1' }));
  });

  it('should dispatch syncCalendar when syncCalendar() is called', () => {
    const dispatchSpy = spyOn(store, 'dispatch');
    component.syncCalendar('cal-123');
    expect(dispatchSpy).toHaveBeenCalledWith(ExternalCalendarsActions.syncCalendar({ id: 'cal-123' }));
  });

  it('syncingId$ should emit null from initial state', done => {
    component.syncingId$.subscribe(id => {
      expect(id).toBeNull();
      done();
    });
  });

  it('syncingId$ should emit calendar id when sync is in progress', done => {
    store.setState({ externalCalendars: { ...initialState.externalCalendars, syncingId: 'cal-abc' } });
    component.syncingId$.subscribe(id => {
      expect(id).toBe('cal-abc');
      done();
    });
  });
});

function mockOf(value: any) { return { subscribe: (fn: any) => fn(value) }; }
