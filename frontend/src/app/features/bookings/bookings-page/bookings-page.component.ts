import { BreakpointObserver } from '@angular/cdk/layout';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { Booking } from '../../../core/models/booking.model';
import { BookingsActions } from '../../../store/bookings/bookings.actions';
import { selectAllBookings, selectBookingsLoading } from '../../../store/bookings/bookings.selectors';

@Component({
  selector: 'app-bookings-page',
  templateUrl: './bookings-page.component.html',
  styleUrls: ['./bookings-page.component.scss'],
})
export class BookingsPageComponent implements OnInit {
  bookings$: Observable<Booking[]> = this.store.select(selectAllBookings);
  loading$: Observable<boolean> = this.store.select(selectBookingsLoading);
  displayedColumns = ['checkIn', 'checkOut', 'guestName', 'status', 'rawSummary'];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private store: Store,
    private breakpointObserver: BreakpointObserver
  ) {}

  ngOnInit(): void {
    const roomId = this.route.snapshot.paramMap.get('roomId')!;
    this.store.dispatch(BookingsActions.loadBookingsByRoom({ roomId }));

    this.breakpointObserver.observe('(max-width: 767px)').subscribe(state => {
      this.displayedColumns = state.matches
        ? ['checkIn', 'checkOut', 'status']
        : ['checkIn', 'checkOut', 'guestName', 'status', 'rawSummary'];
    });
  }

  goBack(): void {
    this.router.navigate(['/external-calendars', this.route.snapshot.paramMap.get('roomId')]);
  }

  statusColor(status: string): string {
    return status === 'Confirmed' ? 'primary' : status === 'Cancelled' ? 'warn' : 'accent';
  }
}
