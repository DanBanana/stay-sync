import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, of, switchMap, tap } from 'rxjs';
import { RoomService } from '../../core/services/room.service';
import { RoomsActions } from './rooms.actions';

@Injectable()
export class RoomsEffects {
  loadRooms$ = createEffect(() =>
    this.actions$.pipe(
      ofType(RoomsActions.loadRooms),
      switchMap(({ propertyId }) =>
        this.roomService.getByProperty(propertyId).pipe(
          map(rooms => RoomsActions.loadRoomsSuccess({ rooms })),
          catchError(err => of(RoomsActions.loadRoomsFailure({ error: err.message })))
        )
      )
    )
  );

  createRoom$ = createEffect(() =>
    this.actions$.pipe(
      ofType(RoomsActions.createRoom),
      switchMap(({ propertyId, name }) =>
        this.roomService.create(propertyId, name).pipe(
          map(room => RoomsActions.createRoomSuccess({ room })),
          catchError(err => of(RoomsActions.createRoomFailure({ error: err.message })))
        )
      )
    )
  );

  updateRoom$ = createEffect(() =>
    this.actions$.pipe(
      ofType(RoomsActions.updateRoom),
      switchMap(({ id, name }) =>
        this.roomService.update(id, name).pipe(
          map(room => RoomsActions.updateRoomSuccess({ room })),
          catchError(err => of(RoomsActions.updateRoomFailure({ error: err.message })))
        )
      )
    )
  );

  deleteRoom$ = createEffect(() =>
    this.actions$.pipe(
      ofType(RoomsActions.deleteRoom),
      switchMap(({ id }) =>
        this.roomService.delete(id).pipe(
          map(() => RoomsActions.deleteRoomSuccess({ id })),
          catchError(err => of(RoomsActions.deleteRoomFailure({ error: err.message })))
        )
      )
    )
  );

  notifyFailure$ = createEffect(() =>
    this.actions$.pipe(
      ofType(
        RoomsActions.createRoomFailure,
        RoomsActions.updateRoomFailure,
        RoomsActions.deleteRoomFailure,
        RoomsActions.loadRoomsFailure,
      ),
      tap(({ error }) => this.snackBar.open(`Error: ${error}`, 'Dismiss', { duration: 5000 }))
    ),
    { dispatch: false }
  );

  constructor(private actions$: Actions, private roomService: RoomService, private snackBar: MatSnackBar) {}
}
