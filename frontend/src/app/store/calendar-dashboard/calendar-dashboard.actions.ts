import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { CalendarBooking } from '../../core/models/booking.model';

export const CalendarDashboardActions = createActionGroup({
  source: 'CalendarDashboard',
  events: {
    'Set Property': props<{ propertyId: string }>(),
    'Navigate Window': props<{ direction: 'forward' | 'backward' }>(),
    'Load Calendar': props<{ propertyId: string; from: string; to: string }>(),
    'Load Calendar Success': props<{ bookings: CalendarBooking[]; from: string; to: string }>(),
    'Load Calendar Failure': props<{ error: string }>(),

    'Create Booking': props<{ roomId: string; checkIn: string; checkOut: string; guestName: string | null }>(),
    'Create Booking Success': emptyProps(),
    'Create Booking Failure': props<{ error: string }>(),

    'Update Booking': props<{ id: string; checkIn: string; checkOut: string; guestName: string | null }>(),
    'Update Booking Success': emptyProps(),
    'Update Booking Failure': props<{ error: string }>(),

    'Delete Booking': props<{ id: string }>(),
    'Delete Booking Success': emptyProps(),
    'Delete Booking Failure': props<{ error: string }>(),

    'Reset': emptyProps(),
  }
});
