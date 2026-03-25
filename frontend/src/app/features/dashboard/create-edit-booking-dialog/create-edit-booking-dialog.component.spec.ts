import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { provideMockStore, MockStore } from '@ngrx/store/testing';
import { CreateEditBookingDialogComponent, CreateEditBookingDialogData } from './create-edit-booking-dialog.component';
import { CalendarDashboardActions } from '../../../store/calendar-dashboard/calendar-dashboard.actions';
import { CalendarBooking } from '../../../core/models/booking.model';

const mockRooms = [
  { roomId: 'r1', roomName: 'Room A' },
  { roomId: 'r2', roomName: 'Room B' },
];

const mockBooking: CalendarBooking = {
  id: 'b1', roomId: 'r1', roomName: 'Room A',
  platform: 'Manual', checkIn: '2026-04-05', checkOut: '2026-04-10',
  status: 'Confirmed', guestName: 'Alice',
};

function createComponent(data: CreateEditBookingDialogData) {
  const dialogRefSpy = jasmine.createSpyObj('MatDialogRef', ['close']);
  TestBed.configureTestingModule({
    declarations: [CreateEditBookingDialogComponent],
    imports: [ReactiveFormsModule],
    schemas: [NO_ERRORS_SCHEMA],
    providers: [
      provideMockStore({}),
      { provide: MAT_DIALOG_DATA, useValue: data },
      { provide: MatDialogRef, useValue: dialogRefSpy },
    ],
  });
  TestBed.overrideTemplate(CreateEditBookingDialogComponent, `
    <form [formGroup]="form">
      <select formControlName="roomId"></select>
      <input formControlName="checkIn" />
      <input formControlName="checkOut" />
      <input formControlName="guestName" />
    </form>
  `);
  TestBed.compileComponents();
  const fixture = TestBed.createComponent(CreateEditBookingDialogComponent);
  const store = TestBed.inject(MockStore);
  spyOn(store, 'dispatch');
  fixture.detectChanges();
  return { fixture, component: fixture.componentInstance, store, dialogRefSpy };
}

describe('CreateEditBookingDialogComponent — create mode', () => {
  it('should create with null date controls when no prefill', () => {
    const { component } = createComponent({ rooms: mockRooms });
    expect(component).toBeTruthy();
    expect(component.isEdit).toBeFalse();
    expect(component.form.get('checkIn')?.value).toBeNull();
    expect(component.form.get('checkOut')?.value).toBeNull();
  });

  it('should prefill roomId and checkIn as Date from prefill data', () => {
    const { component } = createComponent({
      rooms: mockRooms,
      prefill: { roomId: 'r1', checkIn: '2026-04-05' },
    });
    expect(component.form.get('roomId')?.value).toBe('r1');
    const checkIn = component.form.get('checkIn')?.value as Date;
    expect(checkIn).toEqual(new Date(2026, 3, 5));
  });

  it('should default checkOut to day after checkIn when prefill provided', () => {
    const { component } = createComponent({
      rooms: mockRooms,
      prefill: { roomId: 'r1', checkIn: '2026-04-05' },
    });
    const checkOut = component.form.get('checkOut')?.value as Date;
    expect(checkOut).toEqual(new Date(2026, 3, 6));
  });

  it('should dispatch createBooking with YYYY-MM-DD strings on submit', () => {
    const { component, store, dialogRefSpy } = createComponent({ rooms: mockRooms });
    component.form.patchValue({
      roomId: 'r1',
      checkIn: new Date(2026, 3, 5),
      checkOut: new Date(2026, 3, 10),
      guestName: 'Bob',
    });
    component.onSubmit();
    expect(store.dispatch).toHaveBeenCalledWith(
      CalendarDashboardActions.createBooking({
        roomId: 'r1',
        checkIn: '2026-04-05',
        checkOut: '2026-04-10',
        guestName: 'Bob',
      })
    );
    expect(dialogRefSpy.close).toHaveBeenCalled();
  });

  it('should not dispatch if form is invalid', () => {
    const { component, store } = createComponent({ rooms: mockRooms });
    component.onSubmit();
    expect(store.dispatch).not.toHaveBeenCalled();
  });

  it('cross-field: same checkIn and checkOut makes form invalid', () => {
    const { component } = createComponent({ rooms: mockRooms });
    const d = new Date(2026, 3, 5);
    component.form.patchValue({ roomId: 'r1', checkIn: d, checkOut: d });
    expect(component.form.hasError('checkOutNotAfterCheckIn')).toBeTrue();
    expect(component.form.valid).toBeFalse();
  });

  it('cross-field: checkOut before checkIn makes form invalid', () => {
    const { component } = createComponent({ rooms: mockRooms });
    component.form.patchValue({
      roomId: 'r1',
      checkIn: new Date(2026, 3, 10),
      checkOut: new Date(2026, 3, 5),
    });
    expect(component.form.hasError('checkOutNotAfterCheckIn')).toBeTrue();
    expect(component.form.valid).toBeFalse();
  });

  it('cross-field: checkOut after checkIn is valid', () => {
    const { component } = createComponent({ rooms: mockRooms });
    component.form.patchValue({
      roomId: 'r1',
      checkIn: new Date(2026, 3, 5),
      checkOut: new Date(2026, 3, 10),
    });
    expect(component.form.hasError('checkOutNotAfterCheckIn')).toBeFalse();
  });
});

describe('CreateEditBookingDialogComponent — edit mode', () => {
  it('should be in edit mode with date controls as Date objects', () => {
    const { component } = createComponent({ rooms: mockRooms, booking: mockBooking });
    expect(component.isEdit).toBeTrue();
    expect(component.form.get('checkIn')?.value).toEqual(new Date(2026, 3, 5));
    expect(component.form.get('checkOut')?.value).toEqual(new Date(2026, 3, 10));
  });

  it('should populate guestName from booking data', () => {
    const { component } = createComponent({ rooms: mockRooms, booking: mockBooking });
    expect(component.form.get('guestName')?.value).toBe('Alice');
  });

  it('should dispatch updateBooking with YYYY-MM-DD strings on submit', () => {
    const { component, store, dialogRefSpy } = createComponent({ rooms: mockRooms, booking: mockBooking });
    component.form.patchValue({
      checkIn: new Date(2026, 4, 1),
      checkOut: new Date(2026, 4, 7),
      guestName: 'Carol',
    });
    component.onSubmit();
    expect(store.dispatch).toHaveBeenCalledWith(
      CalendarDashboardActions.updateBooking({
        id: 'b1',
        checkIn: '2026-05-01',
        checkOut: '2026-05-07',
        guestName: 'Carol',
      })
    );
    expect(dialogRefSpy.close).toHaveBeenCalled();
  });
});
