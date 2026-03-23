import { TestBed } from '@angular/core/testing';
import { provideMockActions } from '@ngrx/effects/testing';
import { Observable, of, throwError } from 'rxjs';
import { Action } from '@ngrx/store';
import { PropertiesEffects } from './properties.effects';
import { PropertiesActions } from './properties.actions';
import { PropertyService } from '../../core/services/property.service';
import { Property } from '../../core/models/property.model';

const mockProperty: Property = {
  id: '1', name: 'Beach House', address: null, propertyManagerId: 'pm1', createdAt: ''
};

describe('PropertiesEffects', () => {
  let actions$: Observable<Action>;
  let effects: PropertiesEffects;
  let propertyService: jasmine.SpyObj<PropertyService>;

  beforeEach(() => {
    propertyService = jasmine.createSpyObj('PropertyService', ['getAll', 'create', 'update', 'delete']);

    TestBed.configureTestingModule({
      providers: [
        PropertiesEffects,
        provideMockActions(() => actions$),
        { provide: PropertyService, useValue: propertyService },
      ],
    });

    effects = TestBed.inject(PropertiesEffects);
  });

  it('loadProperties$ dispatches success with properties', done => {
    propertyService.getAll.and.returnValue(of([mockProperty]));
    actions$ = of(PropertiesActions.loadProperties());

    effects.loadProperties$.subscribe(action => {
      expect(action).toEqual(PropertiesActions.loadPropertiesSuccess({ properties: [mockProperty] }));
      done();
    });
  });

  it('loadProperties$ dispatches failure on error', done => {
    propertyService.getAll.and.returnValue(throwError(() => new Error('Network error')));
    actions$ = of(PropertiesActions.loadProperties());

    effects.loadProperties$.subscribe(action => {
      expect(action.type).toEqual(PropertiesActions.loadPropertiesFailure.type);
      done();
    });
  });

  it('deleteProperty$ dispatches success with id', done => {
    propertyService.delete.and.returnValue(of(undefined));
    actions$ = of(PropertiesActions.deleteProperty({ id: '1' }));

    effects.deleteProperty$.subscribe(action => {
      expect(action).toEqual(PropertiesActions.deletePropertySuccess({ id: '1' }));
      done();
    });
  });
});
