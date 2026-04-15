import { createReducer, on } from '@ngrx/store';
import { InviteToken } from '../../core/models/invite.model';
import { ManagedUser } from '../../core/models/user-management.model';
import { UsersActions } from './users.actions';

export interface UsersState {
  users: ManagedUser[];
  invites: InviteToken[];
  loading: boolean;
  error: string | null;
  generatedToken: string | null;
}

export const initialState: UsersState = {
  users: [],
  invites: [],
  loading: false,
  error: null,
  generatedToken: null,
};

export const usersReducer = createReducer(
  initialState,

  on(UsersActions.loadUsers, UsersActions.loadInvites, state => ({ ...state, loading: true, error: null })),

  on(UsersActions.loadUsersSuccess, (state, { users }) => ({ ...state, users, loading: false })),
  on(UsersActions.loadInvitesSuccess, (state, { invites }) => ({ ...state, invites, loading: false })),

  on(UsersActions.loadUsersFailure, UsersActions.loadInvitesFailure, (state, { error }) => ({
    ...state, loading: false, error
  })),

  on(UsersActions.deactivateUserSuccess, (state, { id }) => ({
    ...state,
    users: state.users.map(u => u.id === id ? { ...u, isActive: false } : u)
  })),

  on(UsersActions.activateUserSuccess, (state, { id }) => ({
    ...state,
    users: state.users.map(u => u.id === id ? { ...u, isActive: true } : u)
  })),

  on(UsersActions.deleteUserSuccess, (state, { id }) => ({
    ...state,
    users: state.users.filter(u => u.id !== id)
  })),

  on(UsersActions.createInviteSuccess, (state, { invite, result }) => ({
    ...state,
    invites: [invite, ...state.invites],
    generatedToken: result.token,
  })),

  on(UsersActions.revokeInviteSuccess, (state, { id }) => ({
    ...state,
    invites: state.invites.filter(i => i.id !== id)
  })),

  on(UsersActions.clearGeneratedToken, state => ({ ...state, generatedToken: null })),
);
