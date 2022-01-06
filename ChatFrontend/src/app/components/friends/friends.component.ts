import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { ReplaySubject } from 'rxjs';
import { User } from 'src/app/models/user';
import { UserToken } from 'src/app/models/userToken';
import { TokenService } from 'src/app/services/token.service';
import { UserService } from 'src/app/services/user.service';

@Component({
  selector: 'app-friends',
  templateUrl: './friends.component.html',
  styleUrls: ['./friend.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FriendsComponent implements OnInit {
  private readonly token: UserToken;
  friends$: ReplaySubject<Array<User>> = new ReplaySubject<Array<User>>();
  isIncoming: boolean = false;

  constructor(
    tokenService: TokenService,
    private userServices: UserService) {
    this.token = tokenService.token;
  }

  ngOnInit(): void {
    this.getFriends();
  }

  getFriends(): void {
    this.isIncoming = false;
    this.userServices.getFriends(this.token.id).toPromise()
      .then((data: User[]) => {
        this.friends$.next(data);
      });
  }

  incoming(): void {
    this.userServices.getIncoming(this.token.id).toPromise()
      .then((data: User[]) => {
        this.friends$.next(data);
        this.isIncoming = true;
      });
  }

  outgoing(): void {
    this.isIncoming = false;
    this.userServices.getOutgoing(this.token.id).toPromise()
      .then((data: User[]) => {
        this.friends$.next(data);
      });
  }

  accept(fromId: number): void {
    this.userServices.accept(this.token.id, fromId).toPromise()
      .then(() => {
        this.getFriends()
      });
  }
}