import { TestBed } from '@angular/core/testing';
import { provideMockActions } from '@ngrx/effects/testing';
import { Observable, of, throwError } from 'rxjs';
import { Action } from '@ngrx/store';
import { RoomsEffects } from './rooms.effects';
import { RoomsActions } from './rooms.actions';
import { RoomService } from '../../core/services/room.service';
import { Room } from '../../core/models/room.model';

const mockRoom: Room = { id: 'r1', propertyId: 'p1', name: 'Room A', createdAt: '' };

describe('RoomsEffects', () => {
  let actions$: Observable<Action>;
  let effects: RoomsEffects;
  let roomService: jasmine.SpyObj<RoomService>;

  beforeEach(() => {
    roomService = jasmine.createSpyObj('RoomService', ['getByProperty', 'create', 'update', 'delete']);

    TestBed.configureTestingModule({
      providers: [
        RoomsEffects,
        provideMockActions(() => actions$),
        { provide: RoomService, useValue: roomService },
      ],
    });

    effects = TestBed.inject(RoomsEffects);
  });

  it('loadRooms$ dispatches success with rooms', done => {
    roomService.getByProperty.and.returnValue(of([mockRoom]));
    actions$ = of(RoomsActions.loadRooms({ propertyId: 'p1' }));

    effects.loadRooms$.subscribe(action => {
      expect(action).toEqual(RoomsActions.loadRoomsSuccess({ rooms: [mockRoom] }));
      done();
    });
  });

  it('loadRooms$ dispatches failure on error', done => {
    roomService.getByProperty.and.returnValue(throwError(() => new Error('Network error')));
    actions$ = of(RoomsActions.loadRooms({ propertyId: 'p1' }));

    effects.loadRooms$.subscribe(action => {
      expect(action.type).toEqual(RoomsActions.loadRoomsFailure.type);
      done();
    });
  });

  it('createRoom$ dispatches success with room', done => {
    roomService.create.and.returnValue(of(mockRoom));
    actions$ = of(RoomsActions.createRoom({ propertyId: 'p1', name: 'Room A' }));

    effects.createRoom$.subscribe(action => {
      expect(action).toEqual(RoomsActions.createRoomSuccess({ room: mockRoom }));
      done();
    });
  });
});
