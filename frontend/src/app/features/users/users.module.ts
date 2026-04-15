import { ClipboardModule } from '@angular/cdk/clipboard';
import { NgModule } from '@angular/core';
import { EffectsModule } from '@ngrx/effects';
import { SharedModule } from '../../shared/shared.module';
import { UsersEffects } from '../../store/users/users.effects';
import { InviteUserDialogComponent } from './invite-user-dialog/invite-user-dialog.component';
import { UsersPageComponent } from './users-page/users-page.component';
import { UsersRoutingModule } from './users-routing.module';

@NgModule({
  declarations: [UsersPageComponent, InviteUserDialogComponent],
  imports: [
    SharedModule,
    ClipboardModule,
    UsersRoutingModule,
    EffectsModule.forFeature([UsersEffects]),
  ]
})
export class UsersModule {}
