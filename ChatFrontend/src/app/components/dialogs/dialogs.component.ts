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
  public dialogs: ReplaySubject<Array<Dialog>> = new ReplaySubject<Array<Dialog>>();
  public friends$: ReplaySubject<Array<User>> = new ReplaySubject<Array<User>>();
  public friendsDialog: Array<number> = new Array<number>();
  public createShow: boolean = false;
  public dialogName: string = "";
  public userId: number = 0;
  private image: any;

  constructor(
    tokenService: TokenService,
    private chat: ChatService,
    private userServices: UserService) {
    this.userId = tokenService.token.id;
    // this.userServices.getFriends(this.userId).toPromise().then(
    //   (data: User[]) => {
    //     this.friends$.next(data);
    //   }
    // );
  }

  ngOnInit(): void {
    this.getDialogs();
    this.chat.countDialogs.subscribe(
      (data: number) => {
        if(data > 0) {
          this.getDialogs();
        }
      }
    );
  }

  getDialogs() {
    this.chat.getDialogs(this.userId).toPromise()
      .then((data: Dialog[]) => {
        this.dialogs.next(data);
      });
  }

  addDialog() {
    this.createShow = true;
    this.userServices.getFriends(this.userId).toPromise()
      .then((data: User[]) => {
        this.friends$.next(data);
      }
    );
  }

  deleteDialog(id: number) {
    this.chat.deleteDialog(id).toPromise()
      .then(
        _ => {
          this.getDialogs();
        }
      );
  }

  createDialog() {
    if(this.image && this.friendsDialog.length > 0 && this.dialogName.length > 0) {
      this.friendsDialog.push(this.userId);
      let formData: FormData = new FormData();
      formData.append('Name', this.dialogName);
      formData.append('UserId', this.userId.toString());
      formData.append('FacialImage', this.image, this.image.name);
       for (let i = 0; i < this.friendsDialog.length; i++) {
         formData.append(`UsersId[${i}]`, this.friendsDialog[i].toString());
       }
      this.chat.createDialog(formData).toPromise()
        .then(_ => {
          alert("es");
        });
    }
  }

  handleFileInput(event: any): void {
    let image = document.querySelector('.file-name')!;
    if(event.target.files[0]) {
      this.image = event.target.files[0];
      image.innerHTML = this.image!.name;
    } else {
      image.innerHTML = '';
    }
  }

  addUser(friendId: number): void {
    let index = this.friendsDialog.indexOf(friendId);
    if(index == -1) {
      this.friendsDialog.push(friendId);
    }
  }

  removeUser(friendId: number): void {
    let index = this.friendsDialog.indexOf(friendId);
    if(index > -1) {
      this.friendsDialog.splice(index, 1);
    }
  }

  closeDialog() {
    this.friendsDialog = [];
    this.createShow = false;
  }
}
