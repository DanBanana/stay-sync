import { NgModule } from '@angular/core';
import { SharedModule } from '../../shared/shared.module';
import { ExternalCalendarsRoutingModule } from './external-calendars-routing.module';
import { ExternalCalendarsPageComponent } from './external-calendars-page/external-calendars-page.component';
import { CalendarFormDialogComponent } from './calendar-form-dialog/calendar-form-dialog.component';

@NgModule({
  declarations: [ExternalCalendarsPageComponent, CalendarFormDialogComponent],
  imports: [SharedModule, ExternalCalendarsRoutingModule],
})
export class ExternalCalendarsModule {}
