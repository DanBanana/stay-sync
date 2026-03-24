import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { provideMockStore, MockStore } from '@ngrx/store/testing';
import { of } from 'rxjs';
import { DashboardComponent } from './dashboard.component';
import { CalendarDashboardActions } from '../../store/calendar-dashboard/calendar-dashboard.actions';
import { PropertiesActions } from '../../store/properties/properties.actions';
import {
  selectCalendarPropertyId,
  selectCalendarWindowStart,
  selectCalendarWindowEnd,
  selectCalendarDays,
  selectGroupedByRoom,
  selectCalendarLoading,
  selectCalendarError,
} from '../../store/calendar-dashboard/calendar-dashboard.selectors';
import { selectAllProperties } from '../../store/properties/properties.selectors';
import { selectRole } from '../../store/auth/auth.selectors';
import { BookingDetailDialogComponent } from './booking-detail-dialog/booking-detail-dialog.component';
import { CalendarBooking } from '../../core/models/booking.model';

const mockProperty = { id: 'p1', name: 'Beach House', address: null, propertyManagerId: 'pm1', createdAt: '2026-01-01T00:00:00Z' };

const mockBooking: CalendarBooking = {
  id: 'b1', roomId: 'r1', roomName: 'Room A',
  platform: 'Airbnb', checkIn: '2026-04-05', checkOut: '2026-04-10', status: 'Confirmed'
};

const initialState = {};

describe('DashboardComponent', () => {
  let component: DashboardComponent;
  let fixture: ComponentFixture<DashboardComponent>;
  let store: MockStore;
  let dialogSpy: jasmine.SpyObj<MatDialog>;

  beforeEach(async () => {
    dialogSpy = jasmine.createSpyObj('MatDialog', ['open']);

    await TestBed.configureTestingModule({
      declarations: [DashboardComponent],
      schemas: [NO_ERRORS_SCHEMA],
      providers: [
        provideMockStore({ initialState }),
        { provide: MatDialog, useValue: dialogSpy },
      ],
    }).compileComponents();

    store = TestBed.inject(MockStore);

    store.overrideSelector(selectAllProperties, [mockProperty]);
    store.overrideSelector(selectRole, 'PropertyManager');
    store.overrideSelector(selectCalendarPropertyId, null);
    store.overrideSelector(selectCalendarWindowStart, '2026-03-17');
    store.overrideSelector(selectCalendarWindowEnd, '2026-04-28');
    store.overrideSelector(selectCalendarDays, []);
    store.overrideSelector(selectGroupedByRoom, []);
    store.overrideSelector(selectCalendarLoading, false);
    store.overrideSelector(selectCalendarError, null);

    spyOn(store, 'dispatch').and.callThrough();

    fixture = TestBed.createComponent(DashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should dispatch loadProperties on init', () => {
    expect(store.dispatch).toHaveBeenCalledWith(PropertiesActions.loadProperties());
  });

  it('should auto-select first property when none is selected', () => {
    expect(store.dispatch).toHaveBeenCalledWith(
      CalendarDashboardActions.setProperty({ propertyId: 'p1' })
    );
  });

  it('should dispatch setProperty on property change', () => {
    (store.dispatch as jasmine.Spy).calls.reset();
    component.onPropertyChange('p2');
    expect(store.dispatch).toHaveBeenCalledWith(
      CalendarDashboardActions.setProperty({ propertyId: 'p2' })
    );
  });

  it('should dispatch navigateWindow backward', () => {
    (store.dispatch as jasmine.Spy).calls.reset();
    component.navigateBack();
    expect(store.dispatch).toHaveBeenCalledWith(
      CalendarDashboardActions.navigateWindow({ direction: 'backward' })
    );
  });

  it('should dispatch navigateWindow forward', () => {
    (store.dispatch as jasmine.Spy).calls.reset();
    component.navigateForward();
    expect(store.dispatch).toHaveBeenCalledWith(
      CalendarDashboardActions.navigateWindow({ direction: 'forward' })
    );
  });

  it('should open BookingDetailDialog on booking click', () => {
    component.onBookingClicked(mockBooking);
    expect(dialogSpy.open).toHaveBeenCalledWith(BookingDetailDialogComponent, {
      data: mockBooking,
      width: '340px',
    });
  });
});
