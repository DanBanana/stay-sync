import { Clipboard } from '@angular/cdk/clipboard';
import { Component, Inject, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Actions, ofType } from '@ngrx/effects';
import { Store } from '@ngrx/store';
import { Subject, takeUntil } from 'rxjs';
import { UserRole } from '../../../core/models/auth.model';
import { UsersActions } from '../../../store/users/users.actions';
import { selectGeneratedToken } from '../../../store/users/users.selectors';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-invite-user-dialog',
  templateUrl: './invite-user-dialog.component.html',
  styleUrls: ['./invite-user-dialog.component.scss']
})
export class InviteUserDialogComponent implements OnDestroy {
  form: FormGroup;
  submitting = false;
  generatedLink: string | null = null;
  copied = false;

  private readonly destroy$ = new Subject<void>();

  readonly roles: { label: string; value: UserRole }[] = [
    { label: 'Property Manager', value: 'PropertyManager' }
  ];

  constructor(
    private fb: FormBuilder,
    private store: Store,
    private actions$: Actions,
    private clipboard: Clipboard,
    public dialogRef: MatDialogRef<InviteUserDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: {}
  ) {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      role: ['PropertyManager', Validators.required]
    });

    // Watch for generated token
    this.store.select(selectGeneratedToken)
      .pipe(takeUntil(this.destroy$))
      .subscribe(token => {
        if (token) {
          this.submitting = false;
          this.generatedLink = this.buildLink(token);
          this.store.dispatch(UsersActions.clearGeneratedToken());
        }
      });

    // Watch for failure
    this.actions$.pipe(
      ofType(UsersActions.createInviteFailure),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.submitting = false;
    });
  }

  private buildLink(token: string): string {
    const base = window.location.origin;
    return `${base}/register?token=${token}`;
  }

  submit(): void {
    if (this.form.invalid) return;
    this.submitting = true;
    this.store.dispatch(UsersActions.createInvite({
      email: this.form.value.email,
      role: this.form.value.role
    }));
  }

  copyLink(): void {
    if (!this.generatedLink) return;
    this.clipboard.copy(this.generatedLink);
    this.copied = true;
    setTimeout(() => this.copied = false, 2000);
  }

  close(): void {
    this.dialogRef.close();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
