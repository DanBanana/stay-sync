import { NgModule } from '@angular/core';
import { SharedModule } from '../../shared/shared.module';
import { PropertiesRoutingModule } from './properties-routing.module';
import { PropertiesPageComponent } from './properties-page/properties-page.component';
import { PropertyFormDialogComponent } from './property-form-dialog/property-form-dialog.component';
import { RoomFormDialogComponent } from '../rooms/room-form-dialog/room-form-dialog.component';

@NgModule({
  declarations: [PropertiesPageComponent, PropertyFormDialogComponent, RoomFormDialogComponent],
  imports: [SharedModule, PropertiesRoutingModule],
})
export class PropertiesModule {}
