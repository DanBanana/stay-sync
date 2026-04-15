import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, of, switchMap, tap } from 'rxjs';
import { InviteService } from '../../core/services/invite.service';
import { UserManagementService } from '../../core/services/user-management.service';
import { UsersActions } from './users.actions';

@Injectable()
export class UsersEffects {

  loadUsers$ = createEffect(() =>
    this.actions$.pipe(
      ofType(UsersActions.loadUsers),
      switchMap(() =>
        this.userService.getUsers().pipe(
          map(users => UsersActions.loadUsersSuccess({ users })),
          catchError(err => of(UsersActions.loadUsersFailure({ error: err.message })))
        )
      )
    )
  );

  loadInvites$ = createEffect(() =>
    this.actions$.pipe(
      ofType(UsersActions.loadInvites),
      switchMap(() =>
        this.inviteService.getInvites().pipe(
          map(invites => UsersActions.loadInvitesSuccess({ invites })),
          catchError(err => of(UsersActions.loadInvitesFailure({ error: err.message })))
        )
      )
    )
  );

  createInvite$ = createEffect(() =>
    this.actions$.pipe(
      ofType(UsersActions.createInvite),
      switchMap(({ email, role }) =>
        this.inviteService.createInvite(email, role).pipe(
          switchMap(result =>
            this.inviteService.getInvites().pipe(
              map(invites => {
                const invite = invites.find(i => i.email === email.toLowerCase().trim())!;
                return UsersActions.createInviteSuccess({ invite, result });
              })
            )
          ),
          catchError(err => {
            const error = err?.error?.errors?.Email?.[0] ?? err?.error?.title ?? 'Failed to create invite.';
            return of(UsersActions.createInviteFailure({ error }));
          })
        )
      )
    )
  );

  revokeInvite$ = createEffect(() =>
    this.actions$.pipe(
      ofType(UsersActions.revokeInvite),
      switchMap(({ id }) =>
        this.inviteService.revokeInvite(id).pipe(
          map(() => UsersActions.revokeInviteSuccess({ id })),
          catchError(err => of(UsersActions.revokeInviteFailure({ error: err.message })))
        )
      )
    )
  );

  deactivateUser$ = createEffect(() =>
    this.actions$.pipe(
      ofType(UsersActions.deactivateUser),
      switchMap(({ id }) =>
        this.userService.deactivateUser(id).pipe(
          map(() => UsersActions.deactivateUserSuccess({ id })),
          catchError(err => of(UsersActions.deactivateUserFailure({ error: err.message })))
        )
      )
    )
  );

  activateUser$ = createEffect(() =>
    this.actions$.pipe(
      ofType(UsersActions.activateUser),
      switchMap(({ id }) =>
        this.userService.activateUser(id).pipe(
          map(() => UsersActions.activateUserSuccess({ id })),
          catchError(err => of(UsersActions.activateUserFailure({ error: err.message })))
        )
      )
    )
  );

  deleteUser$ = createEffect(() =>
    this.actions$.pipe(
      ofType(UsersActions.deleteUser),
      switchMap(({ id }) =>
        this.userService.deleteUser(id).pipe(
          map(() => UsersActions.deleteUserSuccess({ id })),
          catchError(err => of(UsersActions.deleteUserFailure({ error: err.message })))
        )
      )
    )
  );

  notifySuccess$ = createEffect(() =>
    this.actions$.pipe(
      ofType(
        UsersActions.deactivateUserSuccess,
        UsersActions.activateUserSuccess,
        UsersActions.deleteUserSuccess,
        UsersActions.revokeInviteSuccess,
      ),
      tap(action => {
        const messages: Record<string, string> = {
          '[Users] Deactivate User Success': 'User deactivated.',
          '[Users] Activate User Success': 'User activated.',
          '[Users] Delete User Success': 'User deleted.',
          '[Users] Revoke Invite Success': 'Invite revoked.',
        };
        this.snackBar.open(messages[action.type] ?? 'Done.', 'Dismiss', { duration: 3000 });
      })
    ),
    { dispatch: false }
  );

  notifyFailure$ = createEffect(() =>
    this.actions$.pipe(
      ofType(
        UsersActions.loadUsersFailure,
        UsersActions.deactivateUserFailure,
        UsersActions.activateUserFailure,
        UsersActions.deleteUserFailure,
        UsersActions.createInviteFailure,
        UsersActions.revokeInviteFailure,
      ),
      tap(({ error }) => this.snackBar.open(`Error: ${error}`, 'Dismiss', { duration: 5000 }))
    ),
    { dispatch: false }
  );

  constructor(
    private actions$: Actions,
    private inviteService: InviteService,
    private userService: UserManagementService,
    private snackBar: MatSnackBar
  ) {}
}
