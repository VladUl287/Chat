import { Component, ChangeDetectionStrategy, Input } from '@angular/core';
import { Router } from '@angular/router';
import { ReplaySubject } from 'rxjs';
import { User } from 'src/app/models/user';
import { ChatService } from 'src/app/services/chat.service';
import { TokenService } from 'src/app/services/token.service';

@Component({
  selector: 'app-users',
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UsersComponent {
  @Input() users$: ReplaySubject<Array<User>> = new ReplaySubject<Array<User>>();
  
  constructor(
    private router: Router,
    private chat: ChatService, 
    private tokenService: TokenService) { }

  getDialog(toUserId: number): void {
    let userId = this.tokenService.token.id;
    this.chat.getDialog(userId, toUserId).toPromise().then(
      (data: number) => {
        this.router.navigateByUrl("chat/" + data);
      }
    );
  }

  userPage(userId: number): void {
    this.router.navigateByUrl("user/" + userId);
  }
}
