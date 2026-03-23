import { Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Room } from '../../../core/models/room.model';

export interface RoomDialogData { room?: Room; }

@Component({
  selector: 'app-room-form-dialog',
  template: `
    <h2 mat-dialog-title>{{ isEdit ? 'Edit Room' : 'New Room' }}</h2>
    <mat-dialog-content>
      <form [formGroup]="form" class="dialog-form">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Room Name</mat-label>
          <input matInput formControlName="name" placeholder="e.g. Room A" />
          <mat-error *ngIf="form.get('name')?.hasError('required')">Name is required</mat-error>
        </mat-form-field>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Cancel</button>
      <button mat-flat-button color="primary" (click)="submit()" [disabled]="form.invalid">
        {{ isEdit ? 'Save' : 'Create' }}
      </button>
    </mat-dialog-actions>
  `,
  styles: ['.full-width { width: 100%; } .dialog-form { padding-top: 8px; }']
})
export class RoomFormDialogComponent {
  form: FormGroup;
  get isEdit() { return !!this.data.room; }

  constructor(
    private fb: FormBuilder,
    public dialogRef: MatDialogRef<RoomFormDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: RoomDialogData
  ) {
    this.form = this.fb.group({ name: [data.room?.name ?? '', [Validators.required, Validators.maxLength(255)]] });
  }

  submit(): void { if (this.form.valid) this.dialogRef.close(this.form.value); }
}
