import { Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

@Component({
  selector: 'app-calendar-form-dialog',
  template: `
    <h2 mat-dialog-title>Add ICS Calendar</h2>
    <mat-dialog-content>
      <form [formGroup]="form" class="dialog-form">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Platform</mat-label>
          <mat-select formControlName="platform">
            <mat-option value="Airbnb">Airbnb</mat-option>
            <mat-option value="Booking.com">Booking.com</mat-option>
            <mat-option value="Expedia">Expedia</mat-option>
            <mat-option value="VRBO">VRBO</mat-option>
            <mat-option value="Other">Other</mat-option>
          </mat-select>
          <mat-error *ngIf="form.get('platform')?.hasError('required')">Platform is required</mat-error>
        </mat-form-field>
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>ICS URL</mat-label>
          <input matInput formControlName="icsUrl" placeholder="https://..." />
          <mat-error *ngIf="form.get('icsUrl')?.hasError('required')">URL is required</mat-error>
          <mat-error *ngIf="form.get('icsUrl')?.hasError('pattern')">Must be a valid https:// URL</mat-error>
        </mat-form-field>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Cancel</button>
      <button mat-flat-button color="primary" (click)="submit()" [disabled]="form.invalid">Add</button>
    </mat-dialog-actions>
  `,
  styles: ['.full-width { width: 100%; } .dialog-form { display: flex; flex-direction: column; gap: 8px; padding-top: 8px; }']
})
export class CalendarFormDialogComponent {
  form: FormGroup;

  constructor(
    private fb: FormBuilder,
    public dialogRef: MatDialogRef<CalendarFormDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: {}
  ) {
    this.form = this.fb.group({
      platform: ['', Validators.required],
      icsUrl: ['', [Validators.required, Validators.pattern(/^https?:\/\/.+/)]],
    });
  }

  submit(): void { if (this.form.valid) this.dialogRef.close(this.form.value); }
}
