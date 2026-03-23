import { createActionGroup, props } from '@ngrx/store';
import { Booking } from '../../core/models/booking.model';

export const BookingsActions = createActionGroup({
  source: 'Bookings',
  events: {
    'Load Bookings By Room': props<{ roomId: string }>(),
    'Load Bookings By Property': props<{ propertyId: string }>(),
    'Load Bookings Success': props<{ bookings: Booking[] }>(),
    'Load Bookings Failure': props<{ error: string }>(),
  }
});
