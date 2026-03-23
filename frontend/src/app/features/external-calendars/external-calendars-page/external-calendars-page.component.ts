import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { ExternalCalendar } from '../../../core/models/external-calendar.model';
import { ExternalCalendarsActions } from '../../../store/external-calendars/external-calendars.actions';
import { selectAllCalendars, selectCalendarsLoading } from '../../../store/external-calendars/external-calendars.selectors';
import { CalendarFormDialogComponent } from '../calendar-form-dialog/calendar-form-dialog.component';
import { ConfirmDialogComponent } from '../../../shared/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-external-calendars-page',
  templateUrl: './external-calendars-page.component.html',
  styleUrls: ['./external-calendars-page.component.scss'],
})
export class ExternalCalendarsPageComponent implements OnInit {
  calendars$: Observable<ExternalCalendar[]> = this.store.select(selectAllCalendars);
  loading$: Observable<boolean> = this.store.select(selectCalendarsLoading);
  displayedColumns = ['platform', 'icsUrl', 'lastSyncedAt', 'actions'];
  roomId!: string;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private store: Store,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.roomId = this.route.snapshot.paramMap.get('roomId')!;
    this.store.dispatch(ExternalCalendarsActions.loadCalendars({ roomId: this.roomId }));
  }

  openAddCalendar(): void {
    const ref = this.dialog.open(CalendarFormDialogComponent, { data: {}, width: '440px' });
    ref.afterClosed().subscribe(result => {
      if (result) this.store.dispatch(ExternalCalendarsActions.createCalendar({ roomId: this.roomId, ...result }));
    });
  }

  deleteCalendar(calendar: ExternalCalendar): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: { title: 'Remove Calendar', message: `Remove "${calendar.platform}" calendar? Existing bookings will remain.` },
      width: '360px',
    });
    ref.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        this.store.dispatch(ExternalCalendarsActions.deleteCalendar({ id: calendar.id }));
        this.snackBar.open('Calendar removed', 'Dismiss', { duration: 3000 });
      }
    });
  }

  viewBookings(): void {
    this.router.navigate(['/bookings', this.roomId]);
  }

  goBack(): void {
    this.router.navigate(['/properties']);
  }
}
