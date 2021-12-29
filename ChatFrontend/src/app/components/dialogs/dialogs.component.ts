import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { ReplaySubject } from 'rxjs';
import { Dialog } from 'src/app/models/dialog';
import { User } from 'src/app/models/user';
import { ChatService } from 'src/app/services/chat.service';
import { HubService } from 'src/app/services/hub.service';
import { TokenService } from 'src/app/services/token.service';
import { UserService } from 'src/app/services/user.service';

@Component({
  selector: 'app-dialogs',
  templateUrl: './dialogs.component.html',
  styleUrls: ['./dialogs.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DialogsComponent implements OnInit {
  dialogues: ReplaySubject<Array<Dialog>> = new ReplaySubject<Array<Dialog>>();
  friends$: ReplaySubject<Array<User>> = new ReplaySubject<Array<User>>();
  friendsDialog: Array<User> = new Array<User>();
  userId: number = 0;
  createShow: boolean = true;

  constructor(
    tokenService: TokenService, 
    private hub: HubService, 
    private chat: ChatService,
    private userServices: UserService) {
    this.userId = tokenService.token.id;
    this.userServices.getFriends(this.userId).toPromise().then(
      (data: User[]) => {
        this.friends$.next(data);
      }
    );
  }

  ngOnInit(): void {
    this.getDialogs();

    this.hub.connection.on("ReceiveCountDialogs", (count: number) => {
      if(count > 0) {
        this.getDialogs();
      }
    });
  }

  getDialogs() {
    this.chat.getDialogs(this.userId).toPromise()
      .then((data: Dialog[]) => {
        this.dialogues.next(data);
      });
  }

  createDialog() {
    this.createShow = true;
    this.userServices.getFriends(this.userId).toPromise()
      .then((data: User[]) => {
        this.friends$.next(data);
      }
    );
  }

  addUser(user: User): void {
    let index = this.friendsDialog.indexOf(user);
    if(index == -1) {
      this.friendsDialog.push(user);
    }
  }

  removeUser(user: User): void {
    let index = this.friendsDialog.indexOf(user);
    if(index > -1) {
      this.friendsDialog.splice(index, 1);
    }
  }

  closeDialog() {
    this.friendsDialog = [];
    this.createShow = false;
  }
}
