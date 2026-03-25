import { TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { ConfirmDialogComponent, ConfirmDialogData } from './confirm-dialog.component';

function createComponent(data: ConfirmDialogData) {
  const dialogRefSpy = jasmine.createSpyObj('MatDialogRef', ['close']);
  TestBed.configureTestingModule({
    declarations: [ConfirmDialogComponent],
    schemas: [NO_ERRORS_SCHEMA],
    providers: [
      { provide: MAT_DIALOG_DATA, useValue: data },
      { provide: MatDialogRef, useValue: dialogRefSpy },
    ],
  }).compileComponents();
  const fixture = TestBed.createComponent(ConfirmDialogComponent);
  fixture.detectChanges();
  return { component: fixture.componentInstance, dialogRefSpy };
}

describe('ConfirmDialogComponent', () => {
  it('should create', () => {
    const { component } = createComponent({ title: 'Confirm', message: 'Are you sure?' });
    expect(component).toBeTruthy();
  });

  it('should close with true when confirm() is called', () => {
    const { component, dialogRefSpy } = createComponent({ title: 'Delete', message: 'Delete this?' });
    component.confirm();
    expect(dialogRefSpy.close).toHaveBeenCalledWith(true);
  });
});
