import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { BookingDetailDialogComponent } from './booking-detail-dialog.component';
import { CalendarBooking } from '../../../core/models/booking.model';

const mockBooking: CalendarBooking = {
  id: 'b1', roomId: 'r1', roomName: 'Suite 1',
  platform: 'Airbnb', checkIn: '2026-04-05', checkOut: '2026-04-10', status: 'Confirmed'
};

describe('BookingDetailDialogComponent', () => {
  let component: BookingDetailDialogComponent;
  let fixture: ComponentFixture<BookingDetailDialogComponent>;
  let dialogRefSpy: jasmine.SpyObj<MatDialogRef<BookingDetailDialogComponent>>;

  beforeEach(async () => {
    dialogRefSpy = jasmine.createSpyObj('MatDialogRef', ['close']);

    await TestBed.configureTestingModule({
      declarations: [BookingDetailDialogComponent],
      schemas: [NO_ERRORS_SCHEMA],
      providers: [
        { provide: MAT_DIALOG_DATA, useValue: mockBooking },
        { provide: MatDialogRef, useValue: dialogRefSpy },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(BookingDetailDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should expose the injected booking', () => {
    expect(component.booking).toBe(mockBooking);
  });

  it('should calculate nights correctly (5 nights)', () => {
    expect(component.nights).toBe(5);
  });

  it('should return 1 for a single-night stay', () => {
    component.booking = { ...mockBooking, checkIn: '2026-04-05', checkOut: '2026-04-06' };
    expect(component.nights).toBe(1);
  });

  it('should call dialogRef.close when Close is clicked', () => {
    const btn = fixture.nativeElement.querySelector('button');
    btn.click();
    expect(dialogRefSpy.close).toHaveBeenCalled();
  });
});
