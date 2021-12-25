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
  public repUsers$: ReplaySubject<Array<User>> = new ReplaySubject<Array<User>>();
  searchUser: string = '';

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
        this.repUsers$.next(data);
      });
  }

  search() {
    if (this.searchUser.length === 0) {
      this.getUsers();
    }
    if (this.searchUser.length > 6 && this.repUsers$.observers.length == 0) {
      return;
    }
    if (this.searchUser.length > 2) {
      let load = document.querySelector('.load');
      load?.classList.add('loading');

      this.userService.searchUsers(this.searchUser).toPromise()
        .then((data: User[]) => {
          this.repUsers$.next(data)
        });

      load?.classList.remove('loading');
    }
  }
}
