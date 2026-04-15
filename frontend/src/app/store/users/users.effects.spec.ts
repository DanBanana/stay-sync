import { TestBed } from '@angular/core/testing';
import { MatSnackBar } from '@angular/material/snack-bar';
import { provideMockActions } from '@ngrx/effects/testing';
import { Observable, of, throwError } from 'rxjs';
import { Action } from '@ngrx/store';
import { UsersEffects } from './users.effects';
import { UsersActions } from './users.actions';
import { InviteService } from '../../core/services/invite.service';
import { UserManagementService } from '../../core/services/user-management.service';
import { InviteToken } from '../../core/models/invite.model';
import { ManagedUser } from '../../core/models/user-management.model';

const mockUser: ManagedUser = {
  id: 'u1', email: 'pm@example.com', role: 'PropertyManager', isActive: true, createdAt: '2026-01-01T00:00:00Z'
};

const mockInvite: InviteToken = {
  id: 'i1', email: 'new@example.com', role: 'PropertyManager',
  expiresAt: '2026-01-03T00:00:00Z', usedAt: null, isPending: true
};

describe('UsersEffects', () => {
  let actions$: Observable<Action>;
  let effects: UsersEffects;
  let inviteService: jasmine.SpyObj<InviteService>;
  let userService: jasmine.SpyObj<UserManagementService>;

  beforeEach(() => {
    inviteService = jasmine.createSpyObj('InviteService', ['getInvites', 'createInvite', 'revokeInvite']);
    userService = jasmine.createSpyObj('UserManagementService', ['getUsers', 'deactivateUser', 'activateUser', 'deleteUser']);

    TestBed.configureTestingModule({
      providers: [
        UsersEffects,
        provideMockActions(() => actions$),
        { provide: InviteService, useValue: inviteService },
        { provide: UserManagementService, useValue: userService },
        { provide: MatSnackBar, useValue: { open: () => {} } },
      ]
    });

    effects = TestBed.inject(UsersEffects);
  });

  it('loadUsers$ dispatches success', done => {
    userService.getUsers.and.returnValue(of([mockUser]));
    actions$ = of(UsersActions.loadUsers());

    effects.loadUsers$.subscribe(action => {
      expect(action).toEqual(UsersActions.loadUsersSuccess({ users: [mockUser] }));
      done();
    });
  });

  it('loadUsers$ dispatches failure on error', done => {
    userService.getUsers.and.returnValue(throwError(() => new Error('Network error')));
    actions$ = of(UsersActions.loadUsers());

    effects.loadUsers$.subscribe(action => {
      expect(action.type).toBe(UsersActions.loadUsersFailure.type);
      done();
    });
  });

  it('deactivateUser$ dispatches success', done => {
    userService.deactivateUser.and.returnValue(of(undefined));
    actions$ = of(UsersActions.deactivateUser({ id: 'u1' }));

    effects.deactivateUser$.subscribe(action => {
      expect(action).toEqual(UsersActions.deactivateUserSuccess({ id: 'u1' }));
      done();
    });
  });

  it('deleteUser$ dispatches success', done => {
    userService.deleteUser.and.returnValue(of(undefined));
    actions$ = of(UsersActions.deleteUser({ id: 'u1' }));

    effects.deleteUser$.subscribe(action => {
      expect(action).toEqual(UsersActions.deleteUserSuccess({ id: 'u1' }));
      done();
    });
  });

  it('revokeInvite$ dispatches success', done => {
    inviteService.revokeInvite.and.returnValue(of(undefined));
    actions$ = of(UsersActions.revokeInvite({ id: 'i1' }));

    effects.revokeInvite$.subscribe(action => {
      expect(action).toEqual(UsersActions.revokeInviteSuccess({ id: 'i1' }));
      done();
    });
  });
});
