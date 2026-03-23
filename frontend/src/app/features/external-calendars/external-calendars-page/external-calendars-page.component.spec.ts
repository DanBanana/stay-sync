import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideMockStore, MockStore } from '@ngrx/store/testing';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { RouterTestingModule } from '@angular/router/testing';
import { ActivatedRoute } from '@angular/router';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { ExternalCalendarsPageComponent } from './external-calendars-page.component';
import { ExternalCalendarsActions } from '../../../store/external-calendars/external-calendars.actions';

describe('ExternalCalendarsPageComponent', () => {
  let component: ExternalCalendarsPageComponent;
  let fixture: ComponentFixture<ExternalCalendarsPageComponent>;
  let store: MockStore;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ExternalCalendarsPageComponent],
      imports: [RouterTestingModule],
      providers: [
        provideMockStore({ initialState: { externalCalendars: { calendars: [], loading: false, error: null } } }),
        { provide: MatDialog, useValue: { open: () => ({ afterClosed: () => of(null) }) } },
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
});

function of(value: any) { return { subscribe: (fn: any) => fn(value) }; }
