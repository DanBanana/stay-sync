import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, of, switchMap, tap } from 'rxjs';
import { PropertyService } from '../../core/services/property.service';
import { PropertiesActions } from './properties.actions';

@Injectable()
export class PropertiesEffects {
  loadProperties$ = createEffect(() =>
    this.actions$.pipe(
      ofType(PropertiesActions.loadProperties),
      switchMap(() =>
        this.propertyService.getAll().pipe(
          map(properties => PropertiesActions.loadPropertiesSuccess({ properties })),
          catchError(err => of(PropertiesActions.loadPropertiesFailure({ error: err.message })))
        )
      )
    )
  );

  createProperty$ = createEffect(() =>
    this.actions$.pipe(
      ofType(PropertiesActions.createProperty),
      switchMap(({ name, address, propertyManagerId }) =>
        this.propertyService.create(name, address, propertyManagerId).pipe(
          map(property => PropertiesActions.createPropertySuccess({ property })),
          catchError(err => of(PropertiesActions.createPropertyFailure({ error: err.message })))
        )
      )
    )
  );

  updateProperty$ = createEffect(() =>
    this.actions$.pipe(
      ofType(PropertiesActions.updateProperty),
      switchMap(({ id, name, address }) =>
        this.propertyService.update(id, name, address).pipe(
          map(property => PropertiesActions.updatePropertySuccess({ property })),
          catchError(err => of(PropertiesActions.updatePropertyFailure({ error: err.message })))
        )
      )
    )
  );

  deleteProperty$ = createEffect(() =>
    this.actions$.pipe(
      ofType(PropertiesActions.deleteProperty),
      switchMap(({ id }) =>
        this.propertyService.delete(id).pipe(
          map(() => PropertiesActions.deletePropertySuccess({ id })),
          catchError(err => of(PropertiesActions.deletePropertyFailure({ error: err.message })))
        )
      )
    )
  );

  notifyFailure$ = createEffect(() =>
    this.actions$.pipe(
      ofType(
        PropertiesActions.createPropertyFailure,
        PropertiesActions.updatePropertyFailure,
        PropertiesActions.deletePropertyFailure,
        PropertiesActions.loadPropertiesFailure,
      ),
      tap(({ error }) => this.snackBar.open(`Error: ${error}`, 'Dismiss', { duration: 5000 }))
    ),
    { dispatch: false }
  );

  constructor(private actions$: Actions, private propertyService: PropertyService, private snackBar: MatSnackBar) {}
}
