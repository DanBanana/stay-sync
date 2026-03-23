import { Component, OnInit, ViewChild } from '@angular/core';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { MatSidenav } from '@angular/material/sidenav';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import * as AuthActions from '../../store/auth/auth.actions';
import { selectUser } from '../../store/auth/auth.selectors';

@Component({
  selector: 'app-layout',
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.scss']
})
export class LayoutComponent implements OnInit {
  @ViewChild('sidenav') sidenav!: MatSidenav;

  isMobile$: Observable<boolean>;
  user$ = this.store.select(selectUser);

  constructor(
    private breakpointObserver: BreakpointObserver,
    private store: Store
  ) {
    this.isMobile$ = this.breakpointObserver
      .observe([Breakpoints.XSmall, Breakpoints.Small])
      .pipe(map(result => result.matches));
  }

  ngOnInit(): void {}

  closeSidenavOnMobile(): void {
    this.isMobile$.pipe().subscribe(isMobile => {
      if (isMobile) this.sidenav.close();
    }).unsubscribe();
  }

  signOut(): void {
    this.store.dispatch(AuthActions.logout());
  }
}
