import { BreakpointObserver } from '@angular/cdk/layout';
import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { InviteToken } from '../../../core/models/invite.model';
import { ManagedUser } from '../../../core/models/user-management.model';
import { ConfirmDialogComponent } from '../../../shared/confirm-dialog/confirm-dialog.component';
import { UsersActions } from '../../../store/users/users.actions';
import { selectAllInvites, selectAllUsers, selectPendingInvites, selectUsersLoading } from '../../../store/users/users.selectors';
import { InviteUserDialogComponent } from '../invite-user-dialog/invite-user-dialog.component';

@Component({
  selector: 'app-users-page',
  templateUrl: './users-page.component.html',
  styleUrls: ['./users-page.component.scss']
})
export class UsersPageComponent implements OnInit {
  users$: Observable<ManagedUser[]> = this.store.select(selectAllUsers);
  pendingInvites$: Observable<InviteToken[]> = this.store.select(selectPendingInvites);
  loading$: Observable<boolean> = this.store.select(selectUsersLoading);

  userColumns = ['email', 'role', 'status', 'actions'];
  inviteColumns = ['email', 'role', 'expires', 'actions'];

  constructor(
    private store: Store,
    private dialog: MatDialog,
    private breakpointObserver: BreakpointObserver
  ) {}

  ngOnInit(): void {
    this.store.dispatch(UsersActions.loadUsers());
    this.store.dispatch(UsersActions.loadInvites());

    this.breakpointObserver.observe('(max-width: 767px)').subscribe(state => {
      this.userColumns = state.matches
        ? ['email', 'status', 'actions']
        : ['email', 'role', 'status', 'actions'];
      this.inviteColumns = state.matches
        ? ['email', 'expires', 'actions']
        : ['email', 'role', 'expires', 'actions'];
    });
  }

  openInviteDialog(): void {
    this.dialog.open(InviteUserDialogComponent, {
      width: '440px',
      maxWidth: '95vw',
      data: {}
    });
  }

  toggleActive(user: ManagedUser): void {
    if (user.isActive) {
      const ref = this.dialog.open(ConfirmDialogComponent, {
        data: {
          title: 'Deactivate User',
          message: `Deactivate ${user.email}? They will no longer be able to sign in.`
        },
        width: '360px',
        maxWidth: '95vw'
      });
      ref.afterClosed().subscribe(confirmed => {
        if (confirmed) this.store.dispatch(UsersActions.deactivateUser({ id: user.id }));
      });
    } else {
      this.store.dispatch(UsersActions.activateUser({ id: user.id }));
    }
  }

  deleteUser(user: ManagedUser): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Delete User',
        message: `Permanently delete ${user.email}? This cannot be undone.`
      },
      width: '360px',
      maxWidth: '95vw'
    });
    ref.afterClosed().subscribe(confirmed => {
      if (confirmed) this.store.dispatch(UsersActions.deleteUser({ id: user.id }));
    });
  }

  revokeInvite(invite: InviteToken): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Revoke Invite',
        message: `Revoke invite for ${invite.email}? The link will no longer work.`
      },
      width: '360px',
      maxWidth: '95vw'
    });
    ref.afterClosed().subscribe(confirmed => {
      if (confirmed) this.store.dispatch(UsersActions.revokeInvite({ id: invite.id }));
    });
  }
}
