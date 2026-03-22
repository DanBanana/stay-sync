import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { map, take } from 'rxjs';
import { AppState } from '../../store/app.state';
import { selectRole } from '../../store/auth/auth.selectors';

@Injectable({ providedIn: 'root' })
export class RoleGuard implements CanActivate {
  constructor(private store: Store<AppState>, private router: Router) {}

  canActivate(route: ActivatedRouteSnapshot) {
    const allowedRoles: string[] = route.data['roles'] ?? [];

    return this.store.select(selectRole).pipe(
      take(1),
      map(role => {
        if (role && allowedRoles.includes(role)) return true;
        this.router.navigate(['/dashboard']);
        return false;
      })
    );
  }
}
