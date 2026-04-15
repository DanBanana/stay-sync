import { NO_ERRORS_SCHEMA } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { provideMockStore, MockStore } from '@ngrx/store/testing';
import { provideMockActions } from '@ngrx/effects/testing';
import { Observable, of } from 'rxjs';
import { Action } from '@ngrx/store';
import { Clipboard } from '@angular/cdk/clipboard';
import { InviteUserDialogComponent } from './invite-user-dialog.component';
import { UsersActions } from '../../../store/users/users.actions';
import { SharedModule } from '../../../shared/shared.module';

const initialState = {
  users: { users: [], invites: [], loading: false, error: null, generatedToken: null }
};

describe('InviteUserDialogComponent', () => {
  let component: InviteUserDialogComponent;
  let fixture: ComponentFixture<InviteUserDialogComponent>;
  let store: MockStore;
  let actions$: Observable<Action>;
  let clipboard: jasmine.SpyObj<Clipboard>;

  beforeEach(async () => {
    clipboard = jasmine.createSpyObj('Clipboard', ['copy']);

    await TestBed.configureTestingModule({
      declarations: [InviteUserDialogComponent],
      imports: [SharedModule, NoopAnimationsModule],
      providers: [
        provideMockStore({ initialState }),
        provideMockActions(() => actions$),
        { provide: MatDialogRef, useValue: { close: () => {} } },
        { provide: MAT_DIALOG_DATA, useValue: {} },
        { provide: Clipboard, useValue: clipboard },
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();

    store = TestBed.inject(MockStore);
    fixture = TestBed.createComponent(InviteUserDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create with invalid form initially', () => {
    expect(component).toBeTruthy();
    expect(component.form.invalid).toBeTrue();
  });

  it('should dispatch createInvite on valid submit', () => {
    const dispatchSpy = spyOn(store, 'dispatch');
    component.form.get('email')?.setValue('new@example.com');
    component.form.get('role')?.setValue('PropertyManager');

    component.submit();

    expect(dispatchSpy).toHaveBeenCalledWith(
      UsersActions.createInvite({ email: 'new@example.com', role: 'PropertyManager' })
    );
  });

  it('should show generated link when token arrives in store', () => {
    store.setState({
      users: { ...initialState.users, generatedToken: 'abc123' }
    });
    fixture.detectChanges();

    expect(component.generatedLink).toContain('abc123');
  });

  it('should copy link to clipboard', () => {
    component.generatedLink = 'http://localhost/register?token=abc';
    component.copyLink();
    expect(clipboard.copy).toHaveBeenCalledWith('http://localhost/register?token=abc');
  });
});
