import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { Observable, ReplaySubject } from 'rxjs';
import { Dialog } from 'src/app/models/dialog';
import { User } from 'src/app/models/user';
import { ChatService } from 'src/app/services/chat.service';
import { TokenService } from 'src/app/services/token.service';
import { UserService } from 'src/app/services/user.service';

@Component({
  selector: 'app-dialogs',
  templateUrl: './dialogs.component.html',
  styleUrls: ['./dialogs.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DialogsComponent implements OnInit {
  public dialogs$: ReplaySubject<Array<Dialog>> = new ReplaySubject<Array<Dialog>>();
  public friends$: Promise<Array<User>> | undefined;
  public friendsDialog: Array<number> = new Array<number>();
  public createShow: boolean = false;
  public deleteMode: boolean = false;
  public dialogName: string = "";
  public userId: number = 0;
  private image: File | undefined;

  constructor(
    private tokenService: TokenService,
    private chat: ChatService,
    private userServices: UserService) {}

  ngOnInit(): void {
    this.userId = this.tokenService.token.id;

    this.getDialogs();
    this.chat.countDialogs.subscribe(
      (data: number) => {
        if(data > 0) {
          this.getDialogs();
        }
      });
  }

  getDialogs() {
    this.chat.getDialogs(this.userId).toPromise()
      .then((data: Dialog[]) => {
        this.dialogs$.next(data);
      });
  }

  addDialog() {
    this.createShow = true;
    this.friends$ = this.userServices.getFriends(this.userId).toPromise();
  }

  deleteDialog(id: number) {
    this.chat.deleteDialog(id).toPromise()
      .then(_ => {
          this.getDialogs();
        });
  }

  toogleDeleteMode(): void {
    this.deleteMode = !this.deleteMode;
  }

  createDialog() {
    if(this.friendsDialog.length > 0 && this.dialogName.length > 0) {
      this.friendsDialog.push(this.userId);
      let formData: FormData = new FormData();
      formData.append('Name', this.dialogName);
      formData.append('UserId', this.userId.toString());
      if(this.image) {
        formData.append('FacialImage', this.image, this.image.name);
      }
      for (let i = 0; i < this.friendsDialog.length; i++) {
        formData.append(`UsersId[${i}]`, this.friendsDialog[i].toString());
      }

      this.chat.createDialog(formData).toPromise()
        .then(_ => {
          this.closeDialog();
          this.getDialogs();
        });
    }
  }

  handleFileInput(event: any): void {
    let fileName = document.querySelector('.file-name')!;
    let image = event.target.files[0];
    if(image) {
      this.image = image;
      fileName.innerHTML = image.name;
    } else {
      this.image = undefined;
      fileName.innerHTML = '';
    }
  }

  toogleUser(friendId: number): void {
    let index = this.friendsDialog.indexOf(friendId);
    if(index > -1) {
      this.friendsDialog.splice(index, 1);
    } else {
      this.friendsDialog.push(friendId);
    }
  }

  closeDialog() {
    this.friendsDialog = [];
    this.createShow = false;
  }
}
