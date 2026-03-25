import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialog, MatDialogRef } from '@angular/material/dialog';
import { Store } from '@ngrx/store';
import { CalendarBooking } from '../../../core/models/booking.model';
import { CalendarDashboardActions } from '../../../store/calendar-dashboard/calendar-dashboard.actions';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../../shared/confirm-dialog/confirm-dialog.component';
import { CreateEditBookingDialogComponent } from '../create-edit-booking-dialog/create-edit-booking-dialog.component';

@Component({
  selector: 'app-booking-detail-dialog',
  templateUrl: './booking-detail-dialog.component.html',
  styleUrls: ['./booking-detail-dialog.component.scss'],
})
export class BookingDetailDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<BookingDetailDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public booking: CalendarBooking,
    private dialog: MatDialog,
    private store: Store,
  ) {}

  get nights(): number {
    const checkIn = new Date(this.booking.checkIn);
    const checkOut = new Date(this.booking.checkOut);
    return Math.round((checkOut.getTime() - checkIn.getTime()) / (1000 * 60 * 60 * 24));
  }

  get isManual(): boolean {
    return this.booking.platform === 'Manual';
  }

  onEdit(): void {
    this.dialogRef.close();
    this.dialog.open(CreateEditBookingDialogComponent, {
      data: { rooms: [{ roomId: this.booking.roomId, roomName: this.booking.roomName }], booking: this.booking },
      width: '380px',
    });
  }

  onDelete(): void {
    const ref = this.dialog.open<ConfirmDialogComponent, ConfirmDialogData, boolean>(
      ConfirmDialogComponent,
      { data: { title: 'Delete Booking', message: 'Delete this booking? This cannot be undone.' } }
    );
    ref.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        this.store.dispatch(CalendarDashboardActions.deleteBooking({ id: this.booking.id }));
        this.dialogRef.close();
      }
    });
  }
}
