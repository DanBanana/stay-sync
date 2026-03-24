import { AuthState } from './auth/auth.reducer';
import { CalendarDashboardState } from './calendar-dashboard/calendar-dashboard.reducer';

export interface AppState {
  auth: AuthState;
  calendarDashboard: CalendarDashboardState;
}
