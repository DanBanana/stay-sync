import { createReducer, on } from '@ngrx/store';
import { Room } from '../../core/models/room.model';
import * as AuthActions from '../auth/auth.actions';
import { RoomsActions } from './rooms.actions';

export interface RoomsState {
  rooms: Room[];
  loading: boolean;
  error: string | null;
}

export const initialState: RoomsState = { rooms: [], loading: false, error: null };

export const roomsReducer = createReducer(
  initialState,
  on(RoomsActions.loadRooms, state => ({ ...state, loading: true, error: null })),
  on(RoomsActions.loadRoomsSuccess, (state, { rooms }) => ({ ...state, rooms, loading: false })),
  on(RoomsActions.loadRoomsFailure, (state, { error }) => ({ ...state, error, loading: false })),
  on(RoomsActions.createRoomSuccess, (state, { room }) => ({ ...state, rooms: [...state.rooms, room] })),
  on(RoomsActions.updateRoomSuccess, (state, { room }) => ({
    ...state, rooms: state.rooms.map(r => r.id === room.id ? room : r)
  })),
  on(RoomsActions.deleteRoomSuccess, (state, { id }) => ({
    ...state, rooms: state.rooms.filter(r => r.id !== id)
  })),
  on(AuthActions.logout, () => initialState),
);
