import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, ChangeDetectionStrategy, OnDestroy } from '@angular/core';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { Subscription } from 'rxjs';
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
export class UserPageComponent implements OnInit, OnDestroy {
  public user: Promise<User> | undefined;
  private userId: number = 0;
  private routeSub: Subscription | undefined;

  constructor(
    private router: Router,
    private hub: HubService,
    private token: TokenService,
    private userService: UserService,
    private actRouter: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.routeSub = this.actRouter.params.subscribe(
      (data: Params) => {
        let id: number = +data.id
        if(id == this.token.token.id) {
          this.router.navigateByUrl("");
          return;
        }
        this.userId = id;
      });

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

  ngOnDestroy(): void {
    this.routeSub?.unsubscribe();
  }
}
