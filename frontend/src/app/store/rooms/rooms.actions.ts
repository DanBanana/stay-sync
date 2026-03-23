import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { Room } from '../../core/models/room.model';

export const RoomsActions = createActionGroup({
  source: 'Rooms',
  events: {
    'Load Rooms': props<{ propertyId: string }>(),
    'Load Rooms Success': props<{ rooms: Room[] }>(),
    'Load Rooms Failure': props<{ error: string }>(),
    'Create Room': props<{ propertyId: string; name: string }>(),
    'Create Room Success': props<{ room: Room }>(),
    'Create Room Failure': props<{ error: string }>(),
    'Update Room': props<{ id: string; name: string }>(),
    'Update Room Success': props<{ room: Room }>(),
    'Update Room Failure': props<{ error: string }>(),
    'Delete Room': props<{ id: string }>(),
    'Delete Room Success': props<{ id: string }>(),
    'Delete Room Failure': props<{ error: string }>(),
  }
});
