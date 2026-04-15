import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { InviteToken, CreateInviteResult } from '../../core/models/invite.model';
import { ManagedUser } from '../../core/models/user-management.model';
import { UserRole } from '../../core/models/auth.model';

export const UsersActions = createActionGroup({
  source: 'Users',
  events: {
    // Users
    'Load Users': emptyProps(),
    'Load Users Success': props<{ users: ManagedUser[] }>(),
    'Load Users Failure': props<{ error: string }>(),

    'Deactivate User': props<{ id: string }>(),
    'Deactivate User Success': props<{ id: string }>(),
    'Deactivate User Failure': props<{ error: string }>(),

    'Activate User': props<{ id: string }>(),
    'Activate User Success': props<{ id: string }>(),
    'Activate User Failure': props<{ error: string }>(),

    'Delete User': props<{ id: string }>(),
    'Delete User Success': props<{ id: string }>(),
    'Delete User Failure': props<{ error: string }>(),

    // Invites
    'Load Invites': emptyProps(),
    'Load Invites Success': props<{ invites: InviteToken[] }>(),
    'Load Invites Failure': props<{ error: string }>(),

    'Create Invite': props<{ email: string; role: UserRole }>(),
    'Create Invite Success': props<{ invite: InviteToken; result: CreateInviteResult }>(),
    'Create Invite Failure': props<{ error: string }>(),

    'Revoke Invite': props<{ id: string }>(),
    'Revoke Invite Success': props<{ id: string }>(),
    'Revoke Invite Failure': props<{ error: string }>(),

    'Clear Generated Token': emptyProps(),
  }
});
