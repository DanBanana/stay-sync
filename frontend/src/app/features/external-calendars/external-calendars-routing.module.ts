import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ExternalCalendarsPageComponent } from './external-calendars-page/external-calendars-page.component';

const routes: Routes = [
  { path: ':roomId', component: ExternalCalendarsPageComponent },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ExternalCalendarsRoutingModule {}
