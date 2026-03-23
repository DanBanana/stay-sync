import { Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { PropertyManager } from '../../../core/models/property-manager.model';
import { Property } from '../../../core/models/property.model';

export interface PropertyDialogData {
  property?: Property;
  isSuperAdmin?: boolean;
  propertyManagers?: PropertyManager[];
}

@Component({
  selector: 'app-property-form-dialog',
  templateUrl: './property-form-dialog.component.html',
})
export class PropertyFormDialogComponent {
  form: FormGroup;

  constructor(
    private fb: FormBuilder,
    public dialogRef: MatDialogRef<PropertyFormDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: PropertyDialogData
  ) {
    this.form = this.fb.group({
      name: [data.property?.name ?? '', [Validators.required, Validators.maxLength(255)]],
      address: [data.property?.address ?? ''],
      ...(data.isSuperAdmin && !data.property ? {
        propertyManagerId: [null, Validators.required]
      } : {})
    });
  }

  get isEdit(): boolean { return !!this.data.property; }
  get showManagerDropdown(): boolean { return !!this.data.isSuperAdmin && !this.isEdit; }

  submit(): void {
    if (this.form.valid) this.dialogRef.close(this.form.value);
  }
}
