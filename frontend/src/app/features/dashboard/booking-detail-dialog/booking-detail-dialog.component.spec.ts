import { TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialog, MatDialogRef } from '@angular/material/dialog';
import { provideMockStore, MockStore } from '@ngrx/store/testing';
import { of } from 'rxjs';
import { BookingDetailDialogComponent } from './booking-detail-dialog.component';
import { CalendarDashboardActions } from '../../../store/calendar-dashboard/calendar-dashboard.actions';
import { CalendarBooking } from '../../../core/models/booking.model';

const makeBooking = (platform: string): CalendarBooking => ({
  id: 'b1', roomId: 'r1', roomName: 'Suite 1',
  platform, checkIn: '2026-04-05', checkOut: '2026-04-10', status: 'Confirmed'
});

function createComponent(booking: CalendarBooking) {
  const dialogRefSpy = jasmine.createSpyObj('MatDialogRef', ['close']);
  const dialogSpy = jasmine.createSpyObj('MatDialog', ['open']);
  dialogSpy.open.and.returnValue({ afterClosed: () => of(true) });

  TestBed.configureTestingModule({
    declarations: [BookingDetailDialogComponent],
    schemas: [NO_ERRORS_SCHEMA],
    providers: [
      provideMockStore({}),
      { provide: MAT_DIALOG_DATA, useValue: booking },
      { provide: MatDialogRef, useValue: dialogRefSpy },
      { provide: MatDialog, useValue: dialogSpy },
    ],
  }).compileComponents();

  const fixture = TestBed.createComponent(BookingDetailDialogComponent);
  const store = TestBed.inject(MockStore);
  spyOn(store, 'dispatch');
  fixture.detectChanges();
  return { fixture, component: fixture.componentInstance, store, dialogRefSpy, dialogSpy };
}

describe('BookingDetailDialogComponent', () => {
  it('should create', () => {
    const { component } = createComponent(makeBooking('Airbnb'));
    expect(component).toBeTruthy();
  });

  it('should calculate nights correctly (5 nights)', () => {
    const { component } = createComponent(makeBooking('Airbnb'));
    expect(component.nights).toBe(5);
  });

  it('should return 1 for a single-night stay', () => {
    const { component } = createComponent(makeBooking('Airbnb'));
    component.booking = { ...component.booking, checkIn: '2026-04-05', checkOut: '2026-04-06' };
    expect(component.nights).toBe(1);
  });

  it('should call dialogRef.close when Close is clicked', () => {
    const { fixture, dialogRefSpy } = createComponent(makeBooking('Airbnb'));
    const btn = fixture.nativeElement.querySelector('button');
    btn.click();
    expect(dialogRefSpy.close).toHaveBeenCalled();
  });

  it('should expose isManual=false for non-manual bookings', () => {
    const { component } = createComponent(makeBooking('Airbnb'));
    expect(component.isManual).toBeFalse();
  });

  it('should expose isManual=true for manual bookings', () => {
    const { component } = createComponent(makeBooking('Manual'));
    expect(component.isManual).toBeTrue();
  });

  it('should dispatch deleteBooking and close after confirmed delete', () => {
    const { component, store, dialogRefSpy } = createComponent(makeBooking('Manual'));
    component.onDelete();
    expect(store.dispatch).toHaveBeenCalledWith(
      CalendarDashboardActions.deleteBooking({ id: 'b1' })
    );
    expect(dialogRefSpy.close).toHaveBeenCalled();
  });

  it('should open CreateEditBookingDialog and close self on edit', () => {
    const { component, dialogSpy, dialogRefSpy } = createComponent(makeBooking('Manual'));
    component.onEdit();
    expect(dialogSpy.open).toHaveBeenCalled();
    expect(dialogRefSpy.close).toHaveBeenCalled();
  });
});
