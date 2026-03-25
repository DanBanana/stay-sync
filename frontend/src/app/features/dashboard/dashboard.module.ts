import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '../../shared/shared.module';
import { DashboardComponent } from './dashboard.component';
import { CalendarGanttComponent } from './calendar-gantt/calendar-gantt.component';
import { BookingDetailDialogComponent } from './booking-detail-dialog/booking-detail-dialog.component';
import { CreateEditBookingDialogComponent } from './create-edit-booking-dialog/create-edit-booking-dialog.component';

const routes: Routes = [
  { path: '', component: DashboardComponent }
];

@NgModule({
  declarations: [
    DashboardComponent,
    CalendarGanttComponent,
    BookingDetailDialogComponent,
    CreateEditBookingDialogComponent,
  ],
  imports: [SharedModule, RouterModule.forChild(routes)]
})
export class DashboardModule {}
