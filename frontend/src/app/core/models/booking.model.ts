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
