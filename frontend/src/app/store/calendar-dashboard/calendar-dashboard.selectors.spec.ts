import { selectGroupedByRoom } from './calendar-dashboard.selectors';
import { CalendarBooking } from '../../core/models/booking.model';
import { Room } from '../../core/models/room.model';

function makeBooking(id: string, checkIn: string, checkOut: string, roomId = 'r1'): CalendarBooking {
  return { id, roomId, roomName: 'Room A', platform: 'Manual', checkIn, checkOut, status: 'Confirmed' };
}

function makeRoom(id: string, propertyId: string): Room {
  return { id, name: 'Room A', propertyId, createdAt: '2026-01-01T00:00:00Z' };
}

describe('selectGroupedByRoom projector', () => {
  const project = selectGroupedByRoom.projector;

  it('should return one row per room with no bookings', () => {
    const rooms = [makeRoom('r1', 'p1')];
    const result = project('p1', rooms, []);

    expect(result.length).toBe(1);
    expect(result[0].roomId).toBe('r1');
    expect(result[0].bookings.length).toBe(0);
    expect(result[0].laneCount).toBe(1);
    expect(result[0].conflictCount).toBe(0);
  });

  it('should assign lane 0 and no conflict for a single booking', () => {
    const rooms = [makeRoom('r1', 'p1')];
    const bookings = [makeBooking('b1', '2026-04-05', '2026-04-10')];
    const result = project('p1', rooms, bookings);

    const bar = result[0].bookings[0];
    expect(bar.lane).toBe(0);
    expect(bar.hasConflict).toBeFalse();
    expect(bar.conflictsWith.length).toBe(0);
  });

  it('should assign lane 0 to non-overlapping bookings and no conflicts', () => {
    const rooms = [makeRoom('r1', 'p1')];
    const bookings = [
      makeBooking('b1', '2026-04-01', '2026-04-05'),
      makeBooking('b2', '2026-04-05', '2026-04-10'), // adjacent, not overlapping
      makeBooking('b3', '2026-04-12', '2026-04-15'),
    ];
    const result = project('p1', rooms, bookings);
    const bars = result[0].bookings;

    expect(bars.every(b => b.lane === 0)).toBeTrue();
    expect(bars.every(b => !b.hasConflict)).toBeTrue();
    expect(result[0].laneCount).toBe(1);
    expect(result[0].conflictCount).toBe(0);
  });

  it('should assign lane 1 to an overlapping booking and mark both as conflicts', () => {
    const rooms = [makeRoom('r1', 'p1')];
    const bookings = [
      makeBooking('b1', '2026-04-05', '2026-04-10'),
      makeBooking('b2', '2026-04-07', '2026-04-12'), // overlaps b1
    ];
    const result = project('p1', rooms, bookings);
    const bars = result[0].bookings;

    const b1 = bars.find(b => b.id === 'b1')!;
    const b2 = bars.find(b => b.id === 'b2')!;

    expect(b1.lane).toBe(0);
    expect(b2.lane).toBe(1);
    expect(b1.hasConflict).toBeTrue();
    expect(b2.hasConflict).toBeTrue();
    expect(b1.conflictsWith[0].id).toBe('b2');
    expect(b2.conflictsWith[0].id).toBe('b1');
    expect(result[0].laneCount).toBe(2);
    expect(result[0].conflictCount).toBe(2);
  });

  it('should reuse lane 0 for a third booking that does not overlap the first', () => {
    // A: Apr 1-5, B: Apr 3-7 (overlaps A), C: Apr 6-10 (overlaps B but NOT A)
    const rooms = [makeRoom('r1', 'p1')];
    const bookings = [
      makeBooking('b1', '2026-04-01', '2026-04-05'),
      makeBooking('b2', '2026-04-03', '2026-04-07'), // overlaps b1
      makeBooking('b3', '2026-04-06', '2026-04-10'), // overlaps b2, not b1
    ];
    const result = project('p1', rooms, bookings);
    const bars = result[0].bookings;

    const b1 = bars.find(b => b.id === 'b1')!;
    const b2 = bars.find(b => b.id === 'b2')!;
    const b3 = bars.find(b => b.id === 'b3')!;

    expect(b1.lane).toBe(0);
    expect(b2.lane).toBe(1);
    expect(b3.lane).toBe(0); // fits in lane 0 since b1 ended at Apr 5 < Apr 6
    expect(result[0].laneCount).toBe(2);
  });

  it('should only include rooms for the selected property', () => {
    const rooms = [makeRoom('r1', 'p1'), makeRoom('r2', 'p2')];
    const bookings = [makeBooking('b1', '2026-04-05', '2026-04-10', 'r1')];
    const result = project('p1', rooms, bookings);

    expect(result.length).toBe(1);
    expect(result[0].roomId).toBe('r1');
  });
});
