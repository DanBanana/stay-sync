import { BreakpointObserver } from '@angular/cdk/layout';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { ExternalCalendar } from '../../../core/models/external-calendar.model';
import { ExternalCalendarsActions } from '../../../store/external-calendars/external-calendars.actions';
import { selectAllCalendars, selectCalendarsLoading, selectSyncingId } from '../../../store/external-calendars/external-calendars.selectors';
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
  syncingId$: Observable<string | null> = this.store.select(selectSyncingId);
  displayedColumns = ['platform', 'icsUrl', 'lastSyncedAt', 'status', 'sync', 'actions'];
  roomId!: string;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private store: Store,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    private breakpointObserver: BreakpointObserver
  ) {}

  ngOnInit(): void {
    this.roomId = this.route.snapshot.paramMap.get('roomId')!;
    this.store.dispatch(ExternalCalendarsActions.loadCalendars({ roomId: this.roomId }));

    this.breakpointObserver.observe('(max-width: 767px)').subscribe(state => {
      this.displayedColumns = state.matches
        ? ['platform', 'status', 'sync', 'actions']
        : ['platform', 'icsUrl', 'lastSyncedAt', 'status', 'sync', 'actions'];
    });
  }

  openAddCalendar(): void {
    const ref = this.dialog.open(CalendarFormDialogComponent, { data: {}, width: '440px', maxWidth: '95vw' });
    ref.afterClosed().subscribe(result => {
      if (result) this.store.dispatch(ExternalCalendarsActions.createCalendar({ roomId: this.roomId, ...result }));
    });
  }

  syncCalendar(id: string): void {
    this.store.dispatch(ExternalCalendarsActions.syncCalendar({ id }));
  }

  deleteCalendar(calendar: ExternalCalendar): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: { title: 'Remove Calendar', message: `Remove "${calendar.platform}" calendar? Existing bookings will remain.` },
      width: '360px',
      maxWidth: '95vw',
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
