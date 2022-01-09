import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, ChangeDetectionStrategy, OnDestroy } from '@angular/core';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { ReplaySubject, Subscription } from 'rxjs';
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
  public user$: ReplaySubject<User> = new ReplaySubject<User>();
  private userId: number = 0;
  private routeSub: Subscription | undefined;

  constructor(
    private router: Router,
    private hub: HubService,
    private tokenService: TokenService,
    private userService: UserService,
    private actRouter: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.routeSub = this.actRouter.params.subscribe(
      (data: Params) => {
        let id: number = +data.id
        if(id == this.tokenService.token.id) {
          this.router.navigateByUrl("");
          return;
        }
        this.userId = id;
        this.getUser();
      });
  }

  getUser(): void {
    this.userService.getUser(this.userId).toPromise()
      .then((data: User) => {
        this.user$.next(data);
      })
      .catch((data: HttpErrorResponse) => {
        if(data.status === 404) {
          this.router.navigateByUrl("");
        }
      });
  }

  addUser(): void {
    this.hub.addFriend(this.userId)
      ?.then(_ => this.getUser()); 
  }

  deleteFriend(): void {
    this.userService.deleteFriend(this.userId).toPromise()
      .then(_ => this.getUser());
  }

  accept(): void {
    this.userService.accept(this.tokenService.token.id, this.userId).toPromise()
      .then(_ => this.getUser());
  }

  ngOnDestroy(): void {
    this.routeSub?.unsubscribe();
  }
}