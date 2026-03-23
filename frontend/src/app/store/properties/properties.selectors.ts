import { createFeatureSelector, createSelector } from '@ngrx/store';
import { PropertiesState } from './properties.reducer';

export const selectPropertiesState = createFeatureSelector<PropertiesState>('properties');

export const selectAllProperties = createSelector(selectPropertiesState, s => s.properties);
export const selectSelectedPropertyId = createSelector(selectPropertiesState, s => s.selectedId);
export const selectSelectedProperty = createSelector(
  selectPropertiesState,
  s => s.selectedId ? s.properties.find(p => p.id === s.selectedId) ?? null : null
);
export const selectPropertiesLoading = createSelector(selectPropertiesState, s => s.loading);
export const selectPropertiesError = createSelector(selectPropertiesState, s => s.error);
