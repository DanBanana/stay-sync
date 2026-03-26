import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { By } from '@angular/platform-browser';
import { CalendarGanttComponent } from './calendar-gantt.component';
import { BookingBar, CalendarBooking, RoomRow } from '../../../core/models/booking.model';

function makeDays(startIso: string, count: number): Date[] {
  const days: Date[] = [];
  const start = new Date(startIso);
  for (let i = 0; i < count; i++) {
    const d = new Date(start);
    d.setDate(d.getDate() + i);
    days.push(d);
  }
  return days;
}

function makeBar(overrides: Partial<BookingBar> = {}): BookingBar {
  return {
    id: 'b1', roomId: 'r1', roomName: 'Room A',
    platform: 'Airbnb', checkIn: '2026-04-05', checkOut: '2026-04-10',
    status: 'Confirmed', lane: 0, hasConflict: false, conflictsWith: [],
    ...overrides,
  };
}

function makeRoom(overrides: Partial<RoomRow> = {}): RoomRow {
  return {
    roomId: 'r1', roomName: 'Room A',
    bookings: [makeBar()],
    laneCount: 1,
    conflictCount: 0,
    ...overrides,
  };
}

describe('CalendarGanttComponent', () => {
  let component: CalendarGanttComponent;
  let fixture: ComponentFixture<CalendarGanttComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [CalendarGanttComponent],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(CalendarGanttComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    component.rooms = [];
    component.days = [];
    component.windowStart = '2026-03-17';
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should render a booking bar for each booking', () => {
    component.rooms = [makeRoom()];
    component.days = makeDays('2026-03-17', 42);
    component.windowStart = '2026-03-17';
    fixture.detectChanges();

    const bars = fixture.debugElement.queryAll(By.css('.booking-bar'));
    expect(bars.length).toBe(1);
    const label = bars[0].nativeElement.querySelector('.bar-label');
    expect(label.textContent.trim()).toBe('Airbnb');
  });

  it('should apply platform-airbnb class for Airbnb bookings', () => {
    component.rooms = [makeRoom()];
    component.days = makeDays('2026-03-17', 42);
    component.windowStart = '2026-03-17';
    fixture.detectChanges();

    const bar = fixture.debugElement.query(By.css('.booking-bar'));
    expect(bar.nativeElement.classList).toContain('platform-airbnb');
  });

  it('should mark today header cell with today class', () => {
    const today = new Date();
    const start = new Date(today);
    start.setDate(start.getDate() - 7);
    const startIso = start.toISOString().slice(0, 10);

    component.rooms = [];
    component.days = makeDays(startIso, 42);
    component.windowStart = startIso;
    fixture.detectChanges();

    const todayCells = fixture.debugElement.queryAll(By.css('.gantt-header-cell.today'));
    expect(todayCells.length).toBe(1);
  });

  it('should emit cellClicked when an empty day cell is clicked', () => {
    component.rooms = [makeRoom()];
    component.days = makeDays('2026-03-17', 42);
    component.windowStart = '2026-03-17';
    fixture.detectChanges();

    let emitted: { roomId: string; date: Date } | null = null;
    component.cellClicked.subscribe((e: { roomId: string; date: Date }) => emitted = e);

    const cell = fixture.debugElement.query(By.css('.gantt-day-cell'));
    cell.nativeElement.click();

    expect(emitted!.roomId).toBe('r1');
  });

  it('should emit bookingClicked when bar is clicked', () => {
    const bar = makeBar();
    component.rooms = [makeRoom({ bookings: [bar] })];
    component.days = makeDays('2026-03-17', 42);
    component.windowStart = '2026-03-17';
    fixture.detectChanges();

    let emitted: CalendarBooking | null = null;
    component.bookingClicked.subscribe((b: CalendarBooking) => emitted = b);

    const barEl = fixture.debugElement.query(By.css('.booking-bar'));
    barEl.nativeElement.click();

    expect(emitted!.id).toBe('b1');
  });

  it('should apply conflict class to bars with hasConflict true', () => {
    const conflictBar = makeBar({ hasConflict: true, conflictsWith: [{ id: 'b2', platform: 'Booking.com', checkIn: '2026-04-07', checkOut: '2026-04-12' }] });
    component.rooms = [makeRoom({ bookings: [conflictBar], conflictCount: 1 })];
    component.days = makeDays('2026-03-17', 42);
    component.windowStart = '2026-03-17';
    fixture.detectChanges();

    const barEl = fixture.debugElement.query(By.css('.booking-bar'));
    expect(barEl.nativeElement.classList).toContain('conflict');
  });

  it('should not apply conflict class to bars with hasConflict false', () => {
    component.rooms = [makeRoom()];
    component.days = makeDays('2026-03-17', 42);
    component.windowStart = '2026-03-17';
    fixture.detectChanges();

    const barEl = fixture.debugElement.query(By.css('.booking-bar'));
    expect(barEl.nativeElement.classList).not.toContain('conflict');
  });

  it('should render conflict badge when hasConflict is true', () => {
    const conflictBar = makeBar({ hasConflict: true, conflictsWith: [] });
    component.rooms = [makeRoom({ bookings: [conflictBar], conflictCount: 1 })];
    component.days = makeDays('2026-03-17', 42);
    component.windowStart = '2026-03-17';
    fixture.detectChanges();

    const badge = fixture.debugElement.query(By.css('.conflict-badge'));
    expect(badge).toBeTruthy();
  });

  it('should not render conflict badge when hasConflict is false', () => {
    component.rooms = [makeRoom()];
    component.days = makeDays('2026-03-17', 42);
    component.windowStart = '2026-03-17';
    fixture.detectChanges();

    const badge = fixture.debugElement.query(By.css('.conflict-badge'));
    expect(badge).toBeNull();
  });

  it('rowHeight should return 48px for 1 lane', () => {
    const room = makeRoom({ laneCount: 1 });
    // 2*6 + 1*28 + 0*4 = 40px — with BAR_PADDING=6, BAR_HEIGHT=28, LANE_GAP=4
    const height = component.rowHeight(room);
    expect(height).toBe('40px');
  });

  it('rowHeight should return larger value for 2 lanes', () => {
    const room = makeRoom({ laneCount: 2 });
    // 2*6 + 2*28 + 1*4 = 72px
    const height = component.rowHeight(room);
    expect(height).toBe('72px');
  });

  it('should render room conflict badge when conflictCount > 0', () => {
    component.rooms = [makeRoom({ conflictCount: 2 })];
    component.days = makeDays('2026-03-17', 42);
    component.windowStart = '2026-03-17';
    fixture.detectChanges();

    const badge = fixture.debugElement.query(By.css('.room-conflict-badge'));
    expect(badge).toBeTruthy();
    expect(badge.nativeElement.textContent).toContain('2');
  });
});
