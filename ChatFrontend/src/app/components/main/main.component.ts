import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { ReplaySubject } from 'rxjs';
import { User } from 'src/app/models/user';
import { TokenService } from 'src/app/services/token.service';
import { UserService } from 'src/app/services/user.service';

@Component({
  selector: 'app-main',
  templateUrl: './main.component.html',
  styleUrls: ['./main.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MainComponent implements OnInit {
  public users$: ReplaySubject<Array<User>> = new ReplaySubject<Array<User>>();
  public login: string = '';

  constructor(
    private userService: UserService,
    private tokenService: TokenService) { }

  ngOnInit(): void {
    this.getUsers();
  }

  getUsers(): void {
    let userId: number = this.tokenService.token.id;
    this.userService.getUsers(userId).toPromise()
      .then((data: User[]) => {
        this.users$.next(data);
      });
  }

  search() {
    if (this.login.length === 0) {
      this.getUsers();
    }
    if (this.login.length > 6 && this.users$.observers.length == 0) {
      return;
    }
    if (this.login.length > 2) {
      this.userService.search(this.login).toPromise()
        .then((data: User[]) => {
          this.users$.next(data)
        });
    }
  }
}
