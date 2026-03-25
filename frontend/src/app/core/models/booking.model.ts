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
