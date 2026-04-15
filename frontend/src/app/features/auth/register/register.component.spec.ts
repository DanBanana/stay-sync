import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { RouterTestingModule } from '@angular/router/testing';
import { ActivatedRoute } from '@angular/router';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { of, throwError } from 'rxjs';
import { RegisterComponent } from './register.component';
import { InviteService } from '../../../core/services/invite.service';
import { SharedModule } from '../../../shared/shared.module';

describe('RegisterComponent', () => {
  let component: RegisterComponent;
  let fixture: ComponentFixture<RegisterComponent>;
  let inviteService: jasmine.SpyObj<InviteService>;

  function setupRoute(token: string) {
    return {
      snapshot: { queryParamMap: { get: (_: string) => token } }
    };
  }

  beforeEach(async () => {
    inviteService = jasmine.createSpyObj('InviteService', ['validateToken', 'acceptInvite']);

    await TestBed.configureTestingModule({
      declarations: [RegisterComponent],
      imports: [SharedModule, RouterTestingModule, NoopAnimationsModule],
      providers: [
        { provide: InviteService, useValue: inviteService },
        { provide: ActivatedRoute, useValue: setupRoute('valid-token') },
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();
  });

  function createComponent() {
    fixture = TestBed.createComponent(RegisterComponent);
    component = fixture.componentInstance;
  }

  it('should show token error when token is invalid', () => {
    inviteService.validateToken.and.returnValue(
      throwError(() => ({ error: { title: 'This invite link has expired.' } }))
    );
    createComponent();
    fixture.detectChanges();

    expect(component.tokenError).toBe('This invite link has expired.');
    expect(component.inviteInfo).toBeNull();
  });

  it('should build form when token is valid', () => {
    inviteService.validateToken.and.returnValue(
      of({ email: 'new@example.com', role: 'PropertyManager' })
    );
    createComponent();
    fixture.detectChanges();

    expect(component.inviteInfo).toBeTruthy();
    expect(component.form).toBeTruthy();
    expect(component.form.get('email')?.value).toBe('new@example.com');
  });

  it('should mark form invalid when password is weak', () => {
    inviteService.validateToken.and.returnValue(
      of({ email: 'new@example.com', role: 'PropertyManager' })
    );
    createComponent();
    fixture.detectChanges();

    component.form.get('password')?.setValue('weak');
    expect(component.form.get('password')?.valid).toBeFalse();
  });

  it('should dispatch acceptInvite on valid submit', () => {
    inviteService.validateToken.and.returnValue(
      of({ email: 'new@example.com', role: 'PropertyManager' })
    );
    inviteService.acceptInvite.and.returnValue(of(undefined));
    createComponent();
    fixture.detectChanges();

    component.form.get('name')?.setValue('Jane Doe');
    component.form.get('password')?.setValue('Secure1!');
    component.form.get('confirmPassword')?.setValue('Secure1!');
    component.submit();

    expect(inviteService.acceptInvite).toHaveBeenCalled();
  });

  it('passwordStrength should return strong for a fully qualifying password', () => {
    inviteService.validateToken.and.returnValue(
      of({ email: 'new@example.com', role: 'PropertyManager' })
    );
    createComponent();
    fixture.detectChanges();

    component.form.get('password')?.setValue('Secure123!');
    expect(component.passwordStrength).toBe('strong');
  });
});
