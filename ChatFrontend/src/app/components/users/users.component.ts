import { Component, ChangeDetectionStrategy, Input } from '@angular/core';
import { ReplaySubject } from 'rxjs';
import { User } from 'src/app/models/user';
import { HubService } from 'src/app/services/hub.service';

@Component({
  selector: 'app-users',
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UsersComponent {
  @Input() users$: ReplaySubject<Array<User>> = new ReplaySubject<Array<User>>();
  
  constructor(private hub: HubService) { }

  addAsFriend(user: User): void {
    user.isFriend = true;
    this.hub.addFriend(user.id);
  }
}
