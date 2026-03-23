import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { take } from 'rxjs/operators';
import { PropertyManager } from '../../../core/models/property-manager.model';
import { Property } from '../../../core/models/property.model';
import { PropertyManagerService } from '../../../core/services/property-manager.service';
import { PropertiesActions } from '../../../store/properties/properties.actions';
import {
  selectAllProperties,
  selectPropertiesLoading,
  selectSelectedPropertyId,
} from '../../../store/properties/properties.selectors';
import { RoomsActions } from '../../../store/rooms/rooms.actions';
import { selectAllRooms, selectRoomsLoading } from '../../../store/rooms/rooms.selectors';
import { Room } from '../../../core/models/room.model';
import { PropertyFormDialogComponent } from '../property-form-dialog/property-form-dialog.component';
import { RoomFormDialogComponent } from '../../rooms/room-form-dialog/room-form-dialog.component';
import { ConfirmDialogComponent } from '../../../shared/confirm-dialog/confirm-dialog.component';
import { Router } from '@angular/router';
import { selectRole } from '../../../store/auth/auth.selectors';

@Component({
  selector: 'app-properties-page',
  templateUrl: './properties-page.component.html',
  styleUrls: ['./properties-page.component.scss'],
})
export class PropertiesPageComponent implements OnInit {
  properties$: Observable<Property[]> = this.store.select(selectAllProperties);
  propertiesLoading$: Observable<boolean> = this.store.select(selectPropertiesLoading);
  selectedPropertyId$: Observable<string | null> = this.store.select(selectSelectedPropertyId);
  rooms$: Observable<Room[]> = this.store.select(selectAllRooms);
  roomsLoading$: Observable<boolean> = this.store.select(selectRoomsLoading);

  private propertyManagers: PropertyManager[] = [];
  private isSuperAdmin = false;

  constructor(
    private store: Store,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    private router: Router,
    private propertyManagerService: PropertyManagerService
  ) {}

  ngOnInit(): void {
    this.store.dispatch(PropertiesActions.loadProperties());
    this.store.select(selectRole).pipe(take(1)).subscribe(role => {
      this.isSuperAdmin = role === 'SuperAdmin';
      if (this.isSuperAdmin) {
        this.propertyManagerService.getAll().subscribe(managers => {
          this.propertyManagers = managers;
        });
      }
    });
  }

  selectProperty(property: Property): void {
    this.store.dispatch(PropertiesActions.selectProperty({ id: property.id }));
    this.store.dispatch(RoomsActions.loadRooms({ propertyId: property.id }));
  }

  openCreateProperty(): void {
    const ref = this.dialog.open(PropertyFormDialogComponent, {
      data: { isSuperAdmin: this.isSuperAdmin, propertyManagers: this.propertyManagers },
      width: '400px',
    });
    ref.afterClosed().subscribe(result => {
      if (result) this.store.dispatch(PropertiesActions.createProperty(result));
    });
  }

  openEditProperty(property: Property, event: Event): void {
    event.stopPropagation();
    const ref = this.dialog.open(PropertyFormDialogComponent, { data: { property }, width: '400px' });
    ref.afterClosed().subscribe(result => {
      if (result) this.store.dispatch(PropertiesActions.updateProperty({ id: property.id, ...result }));
    });
  }

  deleteProperty(property: Property, event: Event): void {
    event.stopPropagation();
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: { title: 'Delete Property', message: `Delete "${property.name}"? This will remove all rooms and calendars.` },
      width: '360px',
    });
    ref.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        this.store.dispatch(PropertiesActions.deleteProperty({ id: property.id }));
        this.snackBar.open('Property deleted', 'Dismiss', { duration: 3000 });
      }
    });
  }

  openCreateRoom(propertyId: string): void {
    const ref = this.dialog.open(RoomFormDialogComponent, { data: {}, width: '400px' });
    ref.afterClosed().subscribe(result => {
      if (result) this.store.dispatch(RoomsActions.createRoom({ propertyId, name: result.name }));
    });
  }

  openEditRoom(room: Room, event: Event): void {
    event.stopPropagation();
    const ref = this.dialog.open(RoomFormDialogComponent, { data: { room }, width: '400px' });
    ref.afterClosed().subscribe(result => {
      if (result) this.store.dispatch(RoomsActions.updateRoom({ id: room.id, name: result.name }));
    });
  }

  deleteRoom(room: Room, event: Event): void {
    event.stopPropagation();
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: { title: 'Delete Room', message: `Delete "${room.name}"? This will remove all calendars and bookings.` },
      width: '360px',
    });
    ref.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        this.store.dispatch(RoomsActions.deleteRoom({ id: room.id }));
        this.snackBar.open('Room deleted', 'Dismiss', { duration: 3000 });
      }
    });
  }

  viewCalendars(room: Room): void {
    this.router.navigate(['/external-calendars', room.id]);
  }
}
