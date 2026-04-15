import { NO_ERRORS_SCHEMA } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { provideMockStore, MockStore } from '@ngrx/store/testing';
import { of } from 'rxjs';
import { ManagedUser } from '../../../core/models/user-management.model';
import { UsersActions } from '../../../store/users/users.actions';
import { UsersPageComponent } from './users-page.component';

const mockUser: ManagedUser = {
  id: 'u1', email: 'pm@example.com', role: 'PropertyManager', isActive: true, createdAt: '2026-01-01T00:00:00Z'
};

const initialState = {
  users: { users: [mockUser], invites: [], loading: false, error: null, generatedToken: null }
};

describe('UsersPageComponent', () => {
  let component: UsersPageComponent;
  let fixture: ComponentFixture<UsersPageComponent>;
  let store: MockStore;
  let dialog: jasmine.SpyObj<MatDialog>;

  beforeEach(async () => {
    dialog = jasmine.createSpyObj('MatDialog', ['open']);
    dialog.open.and.returnValue({ afterClosed: () => of(false) } as any);

    await TestBed.configureTestingModule({
      declarations: [UsersPageComponent],
      providers: [
        provideMockStore({ initialState }),
        { provide: MatDialog, useValue: dialog },
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();

    store = TestBed.inject(MockStore);
    fixture = TestBed.createComponent(UsersPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should dispatch loadUsers and loadInvites on init', () => {
    const dispatchSpy = spyOn(store, 'dispatch');
    component.ngOnInit();
    expect(dispatchSpy).toHaveBeenCalledWith(UsersActions.loadUsers());
    expect(dispatchSpy).toHaveBeenCalledWith(UsersActions.loadInvites());
  });

  it('should open invite dialog when openInviteDialog is called', () => {
    component.openInviteDialog();
    expect(dialog.open).toHaveBeenCalled();
  });

  it('should dispatch deactivateUser after confirm', () => {
    dialog.open.and.returnValue({ afterClosed: () => of(true) } as any);
    const dispatchSpy = spyOn(store, 'dispatch');

    component.toggleActive(mockUser);

    expect(dialog.open).toHaveBeenCalled();
    expect(dispatchSpy).toHaveBeenCalledWith(UsersActions.deactivateUser({ id: 'u1' }));
  });

  it('should dispatch deleteUser after confirm', () => {
    dialog.open.and.returnValue({ afterClosed: () => of(true) } as any);
    const dispatchSpy = spyOn(store, 'dispatch');

    component.deleteUser(mockUser);

    expect(dispatchSpy).toHaveBeenCalledWith(UsersActions.deleteUser({ id: 'u1' }));
  });

  it('should dispatch activateUser without dialog for inactive user', () => {
    const dispatchSpy = spyOn(store, 'dispatch');
    component.toggleActive({ ...mockUser, isActive: false });
    expect(dispatchSpy).toHaveBeenCalledWith(UsersActions.activateUser({ id: 'u1' }));
  });
});
