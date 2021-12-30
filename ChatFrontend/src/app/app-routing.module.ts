import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ChatComponent } from './components/chat/chat.component';
import { DialogsComponent } from './components/dialogs/dialogs.component';
import { FriendsComponent } from './components/friends/friends.component';
import { MainComponent } from './components/main/main.component';
import { UserPageComponent } from './components/user-page/user-page.component';
import { AuthGuard } from './guard/auth.guard';

const routes: Routes = [
  { path: '', pathMatch: 'full', component: MainComponent, canActivate: [AuthGuard] },
  { path: 'chat/:id', component: ChatComponent, canActivate: [AuthGuard] },
  { path: 'dialogs', component: DialogsComponent, canActivate: [AuthGuard] },
  { path: 'friends', component: FriendsComponent, canActivate: [AuthGuard] },
  { path: 'user/:id', component: UserPageComponent, canActivate: [AuthGuard] },
  { 
    path: 'auth', 
    loadChildren: () => import('./components/auth/auth.module').then(e => e.AuthModule)
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
