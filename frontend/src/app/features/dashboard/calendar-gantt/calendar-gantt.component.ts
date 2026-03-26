import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { faTriangleExclamation } from '@fortawesome/free-solid-svg-icons';
import { BookingBar, CalendarBooking, RoomRow } from '../../../core/models/booking.model';

@Component({
  selector: 'app-calendar-gantt',
  templateUrl: './calendar-gantt.component.html',
  styleUrls: ['./calendar-gantt.component.scss'],
})
export class CalendarGanttComponent implements OnChanges {
  @Input() rooms: RoomRow[] = [];
  @Input() days: Date[] = [];
  @Input() windowStart!: string;
  @Output() bookingClicked = new EventEmitter<CalendarBooking>();
  @Output() cellClicked = new EventEmitter<{ roomId: string; date: Date }>();

  readonly faTriangleExclamation = faTriangleExclamation;
  readonly BAR_HEIGHT = 28;
  readonly LANE_GAP = 4;
  readonly BAR_PADDING = 6;

  today = new Date();

  ngOnChanges(_changes: SimpleChanges): void {
    this.today = new Date();
  }

  isToday(day: Date): boolean {
    return (
      day.getFullYear() === this.today.getFullYear() &&
      day.getMonth() === this.today.getMonth() &&
      day.getDate() === this.today.getDate()
    );
  }

  bookingsForRoom(roomId: string): BookingBar[] {
    const row = this.rooms.find(r => r.roomId === roomId);
    return row ? row.bookings : [];
  }

  platformClass(platform: string): string {
    const p = platform.toLowerCase();
    if (p.includes('airbnb')) return 'airbnb';
    if (p.includes('booking')) return 'booking';
    if (p.includes('expedia')) return 'expedia';
    if (p.includes('manual')) return 'manual';
    return 'unknown';
  }

  barStyle(booking: BookingBar): { [key: string]: string } {
    const windowStartDate = new Date(this.windowStart);
    const checkIn = new Date(booking.checkIn);
    const checkOut = new Date(booking.checkOut);

    const totalDays = 42;
    const startDay = Math.max(0, this.dayDiff(windowStartDate, checkIn));
    const endDay = Math.min(totalDays, this.dayDiff(windowStartDate, checkOut));
    const spanDays = Math.max(0, endDay - startDay);

    const leftPct = (startDay / totalDays) * 100;
    const widthPct = (spanDays / totalDays) * 100;
    const top = this.BAR_PADDING + booking.lane * (this.BAR_HEIGHT + this.LANE_GAP);

    return {
      left: `${leftPct}%`,
      width: `calc(${widthPct}% - 4px)`,
      top: `${top}px`,
    };
  }

  rowHeight(room: RoomRow): string {
    const height = 2 * this.BAR_PADDING + room.laneCount * this.BAR_HEIGHT + (room.laneCount - 1) * this.LANE_GAP;
    return `${height}px`;
  }

  barTooltip(booking: BookingBar): string {
    if (!booking.hasConflict) {
      return `${booking.platform}\n${this.fmtDate(booking.checkIn)} → ${this.fmtDate(booking.checkOut)}\n${booking.status}`;
    }
    const allBookings = [
      `${booking.platform} · ${this.fmtDate(booking.checkIn)} → ${this.fmtDate(booking.checkOut)}`,
      ...booking.conflictsWith.map(c => `${c.platform} · ${this.fmtDate(c.checkIn)} → ${this.fmtDate(c.checkOut)}`),
    ];
    return `⚠ Conflict detected:\n${allBookings.join('\n')}`;
  }

  private fmtDate(iso: string): string {
    const [year, month, day] = iso.split('-').map(Number);
    return new Date(year, month - 1, day).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
  }

  onBarClick(event: MouseEvent, booking: CalendarBooking): void {
    event.stopPropagation();
    this.bookingClicked.emit(booking);
  }

  private dayDiff(from: Date, to: Date): number {
    const msPerDay = 1000 * 60 * 60 * 24;
    const fromUTC = Date.UTC(from.getFullYear(), from.getMonth(), from.getDate());
    const toUTC = Date.UTC(to.getFullYear(), to.getMonth(), to.getDate());
    return Math.round((toUTC - fromUTC) / msPerDay);
  }
}
