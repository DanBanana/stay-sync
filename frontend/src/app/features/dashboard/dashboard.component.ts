import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Store } from '@ngrx/store';
import { Observable, combineLatest, filter, map, take } from 'rxjs';
import { CalendarBooking } from '../../core/models/booking.model';
import { Property } from '../../core/models/property.model';
import { selectAllProperties } from '../../store/properties/properties.selectors';
import { PropertiesActions } from '../../store/properties/properties.actions';
import { selectRole } from '../../store/auth/auth.selectors';
import { CalendarDashboardActions } from '../../store/calendar-dashboard/calendar-dashboard.actions';
import { RoomsActions } from '../../store/rooms/rooms.actions';
import {
  selectCalendarDays,
  selectCalendarError,
  selectCalendarLoading,
  selectCalendarPropertyId,
  selectCalendarWindowEnd,
  selectCalendarWindowStart,
  selectGroupedByRoom,
} from '../../store/calendar-dashboard/calendar-dashboard.selectors';
import { BookingDetailDialogComponent } from './booking-detail-dialog/booking-detail-dialog.component';
import { CreateEditBookingDialogComponent, CreateEditBookingDialogData } from './create-edit-booking-dialog/create-edit-booking-dialog.component';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
})
export class DashboardComponent implements OnInit {
  properties$: Observable<Property[]> = this.store.select(selectAllProperties);
  role$: Observable<string | null> = this.store.select(selectRole);

  selectedPropertyId$ = this.store.select(selectCalendarPropertyId);
  windowStart$ = this.store.select(selectCalendarWindowStart);
  windowEnd$ = this.store.select(selectCalendarWindowEnd);
  days$ = this.store.select(selectCalendarDays);
  groupedByRoom$ = this.store.select(selectGroupedByRoom);
  loading$ = this.store.select(selectCalendarLoading);
  error$ = this.store.select(selectCalendarError);

  windowLabel$: Observable<string> = combineLatest([this.windowStart$, this.windowEnd$]).pipe(
    map(([start, end]) => {
      const s = new Date(start);
      const e = new Date(end);
      // subtract 1 day from end for display (end is exclusive)
      e.setDate(e.getDate() - 1);
      return `${s.toLocaleDateString('en-GB', { day: 'numeric', month: 'short', year: 'numeric' })} – ${e.toLocaleDateString('en-GB', { day: 'numeric', month: 'short', year: 'numeric' })}`;
    })
  );

  constructor(private store: Store, private dialog: MatDialog) {}

  ngOnInit(): void {
    this.store.dispatch(PropertiesActions.loadProperties());

    // Auto-select the first property once properties load
    this.properties$.pipe(
      filter(props => props.length > 0),
      take(1),
    ).subscribe(props => {
      this.selectedPropertyId$.pipe(take(1)).subscribe(selectedId => {
        if (!selectedId) {
          const propertyId = props[0].id;
          this.store.dispatch(CalendarDashboardActions.setProperty({ propertyId }));
          this.store.dispatch(RoomsActions.loadRooms({ propertyId }));
        }
      });
    });
  }

  onPropertyChange(propertyId: string): void {
    this.store.dispatch(CalendarDashboardActions.setProperty({ propertyId }));
    this.store.dispatch(RoomsActions.loadRooms({ propertyId }));
  }

  navigateBack(): void {
    this.store.dispatch(CalendarDashboardActions.navigateWindow({ direction: 'backward' }));
  }

  navigateForward(): void {
    this.store.dispatch(CalendarDashboardActions.navigateWindow({ direction: 'forward' }));
  }

  onBookingClicked(booking: CalendarBooking): void {
    this.dialog.open(BookingDetailDialogComponent, {
      data: booking,
      width: '380px',
      maxWidth: '95vw',
    });
  }

  onCellClicked(event: { roomId: string; date: Date }, rooms: { roomId: string; roomName: string }[]): void {
    const checkIn = event.date.toISOString().slice(0, 10);
    this.openCreateDialog({ rooms, prefill: { roomId: event.roomId, checkIn } });
  }

  onFabClicked(): void {
    this.groupedByRoom$.pipe(take(1)).subscribe(rooms => {
      this.openCreateDialog({ rooms });
    });
  }

  private openCreateDialog(data: CreateEditBookingDialogData): void {
    this.dialog.open(CreateEditBookingDialogComponent, { data, width: '380px', maxWidth: '95vw' });
  }
}
