import { AuthState } from './auth/auth.reducer';
import { CalendarDashboardState } from './calendar-dashboard/calendar-dashboard.reducer';
import { UsersState } from './users/users.reducer';

export interface AppState {
  auth: AuthState;
  calendarDashboard: CalendarDashboardState;
  users: UsersState;
}
