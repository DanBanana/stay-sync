import { Component, Inject } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ValidationErrors, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Store } from '@ngrx/store';
import { CalendarBooking } from '../../../core/models/booking.model';
import { CalendarDashboardActions } from '../../../store/calendar-dashboard/calendar-dashboard.actions';

function checkInBeforeCheckOut(group: AbstractControl): ValidationErrors | null {
  const checkIn = group.get('checkIn')?.value as Date | null;
  const checkOut = group.get('checkOut')?.value as Date | null;
  if (!checkIn || !checkOut) return null;
  return checkIn < checkOut ? null : { checkOutNotAfterCheckIn: true };
}

export interface CreateEditBookingDialogData {
  rooms: { roomId: string; roomName: string }[];
  prefill?: { roomId: string; checkIn: string };
  booking?: CalendarBooking;
}

@Component({
  selector: 'app-create-edit-booking-dialog',
  templateUrl: './create-edit-booking-dialog.component.html',
  styleUrls: ['./create-edit-booking-dialog.component.scss'],
})
export class CreateEditBookingDialogComponent {
  form: FormGroup;
  isEdit: boolean;

  constructor(
    private fb: FormBuilder,
    public dialogRef: MatDialogRef<CreateEditBookingDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: CreateEditBookingDialogData,
    private store: Store,
  ) {
    this.isEdit = !!data.booking;

    const checkInDate = this.toDate(data.booking?.checkIn ?? data.prefill?.checkIn);
    const checkOutDate = data.booking?.checkOut
      ? this.toDate(data.booking.checkOut)
      : checkInDate
        ? new Date(checkInDate.getFullYear(), checkInDate.getMonth(), checkInDate.getDate() + 1)
        : null;

    this.form = this.fb.group({
      roomId: [{ value: data.booking?.roomId ?? data.prefill?.roomId ?? '', disabled: this.isEdit }, Validators.required],
      checkIn:  [checkInDate, Validators.required],
      checkOut: [checkOutDate, Validators.required],
      guestName: [data.booking?.guestName ?? ''],
    }, { validators: checkInBeforeCheckOut });
  }

  get minCheckOut(): Date | null {
    const checkIn = this.form.get('checkIn')?.value as Date | null;
    if (!checkIn) return null;
    const d = new Date(checkIn);
    d.setDate(d.getDate() + 1);
    return d;
  }

  get maxCheckIn(): Date | null {
    const checkOut = this.form.get('checkOut')?.value as Date | null;
    if (!checkOut) return null;
    const d = new Date(checkOut);
    d.setDate(d.getDate() - 1);
    return d;
  }

  onSubmit(): void {
    if (this.form.invalid) return;
    const { roomId, checkIn, checkOut, guestName } = this.form.getRawValue();
    const checkInStr = this.toDateString(checkIn as Date | null);
    const checkOutStr = this.toDateString(checkOut as Date | null);
    const guestNameVal: string | null = guestName?.trim() || null;

    if (this.isEdit) {
      this.store.dispatch(CalendarDashboardActions.updateBooking({
        id: this.data.booking!.id,
        checkIn: checkInStr,
        checkOut: checkOutStr,
        guestName: guestNameVal,
      }));
    } else {
      this.store.dispatch(CalendarDashboardActions.createBooking({
        roomId,
        checkIn: checkInStr,
        checkOut: checkOutStr,
        guestName: guestNameVal,
      }));
    }
    this.dialogRef.close();
  }

  private toDate(s: string | null | undefined): Date | null {
    if (!s) return null;
    const [y, m, d] = s.split('-').map(Number);
    return new Date(y, m - 1, d);
  }

  private toDateString(d: Date | null): string {
    if (!d) return '';
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
  }
}
