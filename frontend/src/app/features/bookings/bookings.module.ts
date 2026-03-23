import { NgModule } from '@angular/core';
import { SharedModule } from '../../shared/shared.module';
import { BookingsRoutingModule } from './bookings-routing.module';
import { BookingsPageComponent } from './bookings-page/bookings-page.component';

@NgModule({
  declarations: [BookingsPageComponent],
  imports: [SharedModule, BookingsRoutingModule],
})
export class BookingsModule {}
