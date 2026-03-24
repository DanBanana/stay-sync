import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { By } from '@angular/platform-browser';
import { CalendarGanttComponent } from './calendar-gantt.component';
import { CalendarBooking } from '../../../core/models/booking.model';

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

const mockBooking: CalendarBooking = {
  id: 'b1', roomId: 'r1', roomName: 'Room A',
  platform: 'Airbnb', checkIn: '2026-04-05', checkOut: '2026-04-10', status: 'Confirmed'
};

const mockRooms = [{ roomId: 'r1', roomName: 'Room A', bookings: [mockBooking] }];

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
    component.rooms = mockRooms;
    component.days = makeDays('2026-03-17', 42);
    component.windowStart = '2026-03-17';
    fixture.detectChanges();

    const bars = fixture.debugElement.queryAll(By.css('.booking-bar'));
    expect(bars.length).toBe(1);
    expect(bars[0].nativeElement.textContent.trim()).toBe('Airbnb');
  });

  it('should apply platform-airbnb class for Airbnb bookings', () => {
    component.rooms = mockRooms;
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

  it('should emit bookingClicked when bar is clicked', () => {
    component.rooms = mockRooms;
    component.days = makeDays('2026-03-17', 42);
    component.windowStart = '2026-03-17';
    fixture.detectChanges();

    let emitted: CalendarBooking | null = null;
    component.bookingClicked.subscribe((b: CalendarBooking) => emitted = b);

    const bar = fixture.debugElement.query(By.css('.booking-bar'));
    bar.nativeElement.click();

    expect(emitted!).toEqual(mockBooking);
  });
});
