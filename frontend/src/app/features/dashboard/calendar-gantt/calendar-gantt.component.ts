import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { CalendarBooking } from '../../../core/models/booking.model';

interface RoomRow {
  roomId: string;
  roomName: string;
  bookings: CalendarBooking[];
}

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

  bookingsForRoom(roomId: string): CalendarBooking[] {
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

  barStyle(booking: CalendarBooking): { [key: string]: string } {
    const windowStartDate = new Date(this.windowStart);
    const checkIn = new Date(booking.checkIn);
    const checkOut = new Date(booking.checkOut);

    const totalDays = 42;
    const startDay = Math.max(0, this.dayDiff(windowStartDate, checkIn));
    const endDay = Math.min(totalDays, this.dayDiff(windowStartDate, checkOut));
    const spanDays = Math.max(0, endDay - startDay);

    const leftPct = (startDay / totalDays) * 100;
    const widthPct = (spanDays / totalDays) * 100;

    return {
      left: `${leftPct}%`,
      width: `calc(${widthPct}% - 4px)`,
    };
  }

  barTooltip(booking: CalendarBooking): string {
    return `${booking.platform} · ${booking.checkIn} → ${booking.checkOut} · ${booking.status}`;
  }

  private dayDiff(from: Date, to: Date): number {
    const msPerDay = 1000 * 60 * 60 * 24;
    const fromUTC = Date.UTC(from.getFullYear(), from.getMonth(), from.getDate());
    const toUTC = Date.UTC(to.getFullYear(), to.getMonth(), to.getDate());
    return Math.round((toUTC - fromUTC) / msPerDay);
  }
}
