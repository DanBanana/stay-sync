import { createFeatureSelector, createSelector } from '@ngrx/store';
import { selectAllRooms } from '../rooms/rooms.selectors';
import { CalendarDashboardState } from './calendar-dashboard.reducer';

export const selectCalendarDashboardState = createFeatureSelector<CalendarDashboardState>('calendarDashboard');

export const selectCalendarPropertyId = createSelector(selectCalendarDashboardState, s => s.selectedPropertyId);
export const selectCalendarWindowStart = createSelector(selectCalendarDashboardState, s => s.windowStart);
export const selectCalendarLoadedRange = createSelector(selectCalendarDashboardState, s => s.loadedRange);
export const selectCalendarLoading = createSelector(selectCalendarDashboardState, s => s.loading);
export const selectCalendarSaving = createSelector(selectCalendarDashboardState, s => s.saving);
export const selectCalendarError = createSelector(selectCalendarDashboardState, s => s.error);

export const selectCalendarWindowEnd = createSelector(selectCalendarWindowStart, windowStart => {
  const d = new Date(windowStart);
  d.setDate(d.getDate() + 42);
  return d.toISOString().slice(0, 10);
});

export const selectCalendarDays = createSelector(selectCalendarWindowStart, windowStart => {
  const days: Date[] = [];
  const start = new Date(windowStart);
  for (let i = 0; i < 42; i++) {
    const d = new Date(start);
    d.setDate(d.getDate() + i);
    days.push(d);
  }
  return days;
});

export const selectBookingsInWindow = createSelector(
  selectCalendarDashboardState,
  selectCalendarWindowStart,
  selectCalendarWindowEnd,
  (state, windowStart, windowEnd) =>
    state.bookings.filter(b => b.checkIn < windowEnd && b.checkOut > windowStart)
);

export const selectGroupedByRoom = createSelector(
  selectCalendarPropertyId,
  selectAllRooms,
  selectBookingsInWindow,
  (propertyId, rooms, bookings) => {
    const propertyRooms = rooms.filter(r => r.propertyId === propertyId);
    const bookingMap = new Map<string, typeof bookings>();
    for (const b of bookings) {
      if (!bookingMap.has(b.roomId)) bookingMap.set(b.roomId, []);
      bookingMap.get(b.roomId)!.push(b);
    }
    return propertyRooms
      .map(r => ({ roomId: r.id, roomName: r.name, bookings: bookingMap.get(r.id) ?? [] }))
      .sort((a, b) => a.roomName.localeCompare(b.roomName));
  }
);

export const selectNeedsLoad = createSelector(
  selectCalendarWindowStart,
  selectCalendarWindowEnd,
  selectCalendarLoadedRange,
  (windowStart, windowEnd, loadedRange) => {
    if (!loadedRange) return { needed: true, from: windowStart, to: windowEnd };
    if (windowStart < loadedRange.from) return { needed: true, from: windowStart, to: loadedRange.from };
    if (windowEnd > loadedRange.to) return { needed: true, from: loadedRange.to, to: windowEnd };
    return { needed: false, from: windowStart, to: windowEnd };
  }
);
