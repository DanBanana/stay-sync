import { createAction, createActionGroup, emptyProps, props } from '@ngrx/store';
import { Property } from '../../core/models/property.model';

export const PropertiesActions = createActionGroup({
  source: 'Properties',
  events: {
    'Load Properties': emptyProps(),
    'Load Properties Success': props<{ properties: Property[] }>(),
    'Load Properties Failure': props<{ error: string }>(),
    'Create Property': props<{ name: string; address?: string; propertyManagerId?: string }>(),
    'Create Property Success': props<{ property: Property }>(),
    'Create Property Failure': props<{ error: string }>(),
    'Update Property': props<{ id: string; name: string; address?: string }>(),
    'Update Property Success': props<{ property: Property }>(),
    'Update Property Failure': props<{ error: string }>(),
    'Delete Property': props<{ id: string }>(),
    'Delete Property Success': props<{ id: string }>(),
    'Delete Property Failure': props<{ error: string }>(),
    'Select Property': props<{ id: string | null }>(),
  }
});
