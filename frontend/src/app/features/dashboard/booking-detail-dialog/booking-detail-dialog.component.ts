import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { CalendarBooking } from '../../../core/models/booking.model';

@Component({
  selector: 'app-booking-detail-dialog',
  templateUrl: './booking-detail-dialog.component.html',
})
export class BookingDetailDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<BookingDetailDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public booking: CalendarBooking
  ) {}

  get nights(): number {
    const checkIn = new Date(this.booking.checkIn);
    const checkOut = new Date(this.booking.checkOut);
    return Math.round((checkOut.getTime() - checkIn.getTime()) / (1000 * 60 * 60 * 24));
  }
}
