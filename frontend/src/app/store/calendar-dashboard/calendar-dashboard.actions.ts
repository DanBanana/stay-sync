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
    'Reset': emptyProps(),
  }
});
