import { NgModule, isDevMode } from '@angular/core';
import { ServiceWorkerModule } from '@angular/service-worker';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { StoreModule } from '@ngrx/store';
import { EffectsModule } from '@ngrx/effects';
import { StoreDevtoolsModule } from '@ngrx/store-devtools';
import { localStorageSync } from 'ngrx-store-localstorage';
import { ActionReducer, MetaReducer } from '@ngrx/store';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { CoreModule } from './core/core.module';
import { authReducer } from './store/auth/auth.reducer';
import { AuthEffects } from './store/auth/auth.effects';
import { propertiesReducer } from './store/properties/properties.reducer';
import { PropertiesEffects } from './store/properties/properties.effects';
import { roomsReducer } from './store/rooms/rooms.reducer';
import { RoomsEffects } from './store/rooms/rooms.effects';
import { externalCalendarsReducer } from './store/external-calendars/external-calendars.reducer';
import { ExternalCalendarsEffects } from './store/external-calendars/external-calendars.effects';
import { bookingsReducer } from './store/bookings/bookings.reducer';
import { BookingsEffects } from './store/bookings/bookings.effects';
import { calendarDashboardReducer } from './store/calendar-dashboard/calendar-dashboard.reducer';
import { CalendarDashboardEffects } from './store/calendar-dashboard/calendar-dashboard.effects';

export function localStorageSyncReducer(reducer: ActionReducer<any>): ActionReducer<any> {
  return localStorageSync({ keys: ['auth'], rehydrate: true })(reducer);
}

const metaReducers: MetaReducer[] = [localStorageSyncReducer];

@NgModule({
  declarations: [AppComponent],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    AppRoutingModule,
    CoreModule,
    StoreModule.forRoot({
      auth: authReducer,
      properties: propertiesReducer,
      rooms: roomsReducer,
      externalCalendars: externalCalendarsReducer,
      bookings: bookingsReducer,
      calendarDashboard: calendarDashboardReducer,
    }, { metaReducers }),
    EffectsModule.forRoot([AuthEffects, PropertiesEffects, RoomsEffects, ExternalCalendarsEffects, BookingsEffects, CalendarDashboardEffects]),
    StoreDevtoolsModule.instrument({ maxAge: 25, logOnly: !isDevMode() }),
    ServiceWorkerModule.register('ngsw-worker.js', {
      enabled: !isDevMode(),
      registrationStrategy: 'registerWhenStable:30000'
    })
  ],
  bootstrap: [AppComponent]
})
export class AppModule {}
