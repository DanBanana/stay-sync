import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from './core/auth/auth.guard';
import { LayoutComponent } from './core/layout/layout.component';

const routes: Routes = [
  { path: '', redirectTo: 'properties', pathMatch: 'full' },
  {
    path: 'login',
    loadChildren: () => import('./features/auth/auth.module').then(m => m.AuthModule)
  },
  {
    path: '',
    component: LayoutComponent,
    canActivate: [AuthGuard],
    children: [
      {
        path: 'properties',
        loadChildren: () => import('./features/properties/properties.module').then(m => m.PropertiesModule)
      },
      {
        path: 'rooms',
        loadChildren: () => import('./features/rooms/rooms.module').then(m => m.RoomsModule)
      },
      {
        path: 'external-calendars',
        loadChildren: () => import('./features/external-calendars/external-calendars.module').then(m => m.ExternalCalendarsModule)
      },
      {
        path: 'bookings',
        loadChildren: () => import('./features/bookings/bookings.module').then(m => m.BookingsModule)
      },
      {
        path: 'dashboard',
        loadChildren: () => import('./features/dashboard/dashboard.module').then(m => m.DashboardModule)
      }
    ]
  },
  { path: '**', redirectTo: 'properties' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
