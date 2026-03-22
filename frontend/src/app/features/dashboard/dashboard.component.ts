import { Component } from '@angular/core';
import { Store } from '@ngrx/store';
import { AppState } from '../../store/app.state';
import { selectUser } from '../../store/auth/auth.selectors';
import { logout } from '../../store/auth/auth.actions';

@Component({
  selector: 'app-dashboard',
  template: `
    <div style="padding: 2rem;">
      <h1>Dashboard</h1>
      <p *ngIf="user$ | async as user">
        Welcome, {{ user.email }} ({{ user.role }})
      </p>
      <button (click)="logout()">Sign out</button>
    </div>
  `
})
export class DashboardComponent {
  user$ = this.store.select(selectUser);

  constructor(private store: Store<AppState>) {}

  logout() {
    this.store.dispatch(logout());
  }
}
