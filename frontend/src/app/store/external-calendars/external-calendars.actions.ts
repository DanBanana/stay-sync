import { createActionGroup, props } from '@ngrx/store';
import { ExternalCalendar, SyncCalendarResult } from '../../core/models/external-calendar.model';

export const ExternalCalendarsActions = createActionGroup({
  source: 'ExternalCalendars',
  events: {
    'Load Calendars': props<{ roomId: string }>(),
    'Load Calendars Success': props<{ calendars: ExternalCalendar[] }>(),
    'Load Calendars Failure': props<{ error: string }>(),
    'Create Calendar': props<{ roomId: string; platform: string; icsUrl: string }>(),
    'Create Calendar Success': props<{ calendar: ExternalCalendar }>(),
    'Create Calendar Failure': props<{ error: string }>(),
    'Delete Calendar': props<{ id: string }>(),
    'Delete Calendar Success': props<{ id: string }>(),
    'Delete Calendar Failure': props<{ error: string }>(),
    'Sync Calendar': props<{ id: string }>(),
    'Sync Calendar Success': props<{ id: string; result: SyncCalendarResult }>(),
    'Sync Calendar Failure': props<{ error: string }>(),
  }
});
