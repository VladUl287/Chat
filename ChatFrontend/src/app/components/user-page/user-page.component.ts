import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { User } from 'src/app/models/user';
import { HubService } from 'src/app/services/hub.service';
import { TokenService } from 'src/app/services/token.service';
import { UserService } from 'src/app/services/user.service';

@Component({
  selector: 'app-user-page',
  templateUrl: './user-page.component.html',
  styleUrls: ['./user-page.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UserPageComponent implements OnInit {
  public user: Promise<User> | undefined;
  private userId: number = 0;

  constructor(
    actRouter: ActivatedRoute,
    private router: Router,
    private hub: HubService,
    private token: TokenService,
    private userService: UserService
  ) {
    actRouter.params.subscribe(
      data => {
        let id: number = +data.id
        if(id == token.token.id) {
          this.router.navigateByUrl("");
          return;
        }
        this.userId = id;
      }
    );
  }

  ngOnInit(): void {
    this.user = this.userService.getUser(this.userId).toPromise();
    this.user.catch(
        (data: HttpErrorResponse) => {
          if(data.status === 404) {
            this.router.navigateByUrl("");
          }
        }
    );
  }

  addUser() {
    this.hub.addFriend(this.userId);
  }

  deleteFriend() {
    this.userService.deleteFriend(this.userId).toPromise()
      .then(_ => this.router.navigateByUrl("friends"));
  }

  acceptBid() {
    this.userService.accept(this.token.token.id, this.userId)
    .subscribe(data => console.log(data));
  }
}
