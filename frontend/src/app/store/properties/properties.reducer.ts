import { createReducer, on } from '@ngrx/store';
import { Property } from '../../core/models/property.model';
import * as AuthActions from '../auth/auth.actions';
import { PropertiesActions } from './properties.actions';

export interface PropertiesState {
  properties: Property[];
  selectedId: string | null;
  loading: boolean;
  error: string | null;
}

export const initialState: PropertiesState = {
  properties: [],
  selectedId: null,
  loading: false,
  error: null,
};

export const propertiesReducer = createReducer(
  initialState,
  on(PropertiesActions.loadProperties, state => ({ ...state, loading: true, error: null })),
  on(PropertiesActions.loadPropertiesSuccess, (state, { properties }) => ({ ...state, properties, loading: false })),
  on(PropertiesActions.loadPropertiesFailure, (state, { error }) => ({ ...state, error, loading: false })),

  on(PropertiesActions.createPropertySuccess, (state, { property }) => ({
    ...state, properties: [...state.properties, property]
  })),
  on(PropertiesActions.updatePropertySuccess, (state, { property }) => ({
    ...state, properties: state.properties.map(p => p.id === property.id ? property : p)
  })),
  on(PropertiesActions.deletePropertySuccess, (state, { id }) => ({
    ...state,
    properties: state.properties.filter(p => p.id !== id),
    selectedId: state.selectedId === id ? null : state.selectedId
  })),
  on(PropertiesActions.selectProperty, (state, { id }) => ({ ...state, selectedId: id })),
  on(AuthActions.logout, () => initialState),
);
