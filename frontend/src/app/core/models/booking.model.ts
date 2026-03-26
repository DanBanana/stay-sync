export interface Booking {
  id: string;
  roomId: string;
  externalCalendarId: string;
  externalUid: string;
  guestName: string | null;
  checkIn: string;
  checkOut: string;
  status: 'Confirmed' | 'Cancelled' | 'Blocked';
  rawSummary: string | null;
}

export interface CalendarBooking {
  id: string;
  roomId: string;
  roomName: string;
  platform: string;
  checkIn: string;
  checkOut: string;
  status: string;
  guestName?: string | null;
}

export interface ConflictInfo {
  id: string;
  platform: string;
  checkIn: string;
  checkOut: string;
}

export interface BookingBar extends CalendarBooking {
  lane: number;
  hasConflict: boolean;
  conflictsWith: ConflictInfo[];
}

export interface RoomRow {
  roomId: string;
  roomName: string;
  bookings: BookingBar[];
  laneCount: number;
  conflictCount: number;
}

export interface CreateManualBookingRequest {
  roomId: string;
  checkIn: string;
  checkOut: string;
  guestName?: string | null;
}

export interface UpdateManualBookingRequest {
  checkIn: string;
  checkOut: string;
  guestName?: string | null;
}
