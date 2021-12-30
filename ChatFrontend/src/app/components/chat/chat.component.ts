import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Observable, ReplaySubject } from 'rxjs';
import { Message } from 'src/app/models/message';
import { MessageModel } from 'src/app/models/messageModel';
import { User } from 'src/app/models/user';
import { ChatService } from 'src/app/services/chat.service';
import { HubService } from 'src/app/services/hub.service';
import { TokenService } from 'src/app/services/token.service';
import { UserService } from 'src/app/services/user.service';

@Component({
  selector: 'app-chat',
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChatComponent implements OnInit, OnDestroy {
  public messages: Array<Message> = new Array<Message>();
  public messages$: ReplaySubject<Array<Message>> = new ReplaySubject<Array<Message>>();
  public selectedMessages: Array<Message> = new Array<Message>(); 
  public messageText: string = '';
  public userId: number = 0;
  public user: User | undefined;
  public controlShow: boolean = false;
  private dialogId: number = 0;

  constructor(
    actRoute: ActivatedRoute,
    private hub: HubService,
    private cd: ChangeDetectorRef,
    private userService: UserService,
    private chatService: ChatService,
    private tokenService: TokenService) {
    actRoute.params.subscribe(
      data => {
        this.dialogId = +data.id;
      }
    );
  }

  ngOnInit(): void {
    this.hub.connection.on("ReceiveMessage", (message: Message) => {
      this.messages.push(message);

      if (this.userId != message.userId) {
        this.hub.checkDialog(this.dialogId);
        this.chatService.getCount(this.userId);
      }
      this.cd.detectChanges();
    });

    this.userId = this.tokenService.token.id;
    this.userService.getUser(this.userId).toPromise()
      .then(
        (data: User) => {
          this.user = data;
          this.cd.detectChanges();
        }
      );
    // this.messages$ = this.chatService.getMessages(this.dialogId);
    // this.chatService.getCount(this.userId);
    this.chatService.getMessages(this.dialogId).toPromise()
      .then((data: Message[]) => {
        this.chatService.getCount(this.userId);
        this.messages = data;
        this.cd.detectChanges();
      });
  }

  getMessages() {
    this.chatService.getMessages(this.dialogId).toPromise()
      .then(
        (data: Message[]) => {
          this.messages = data;
          this.messages$.next(data);
          this.chatService.getCount(this.userId);
        }
      );
  }

  send(): void {
    let message: MessageModel = new MessageModel(this.messageText, this.userId, this.dialogId);
    this.hub.sendMessage(message);
  }

  dialogControlShow() {
    this.controlShow = true;
  }

  dialogControlHide() {
    this.controlShow = false;
  }

  scrollToBottom(): void {
    let objDiv = document.querySelector(".message-zone");
    objDiv!.scrollTop = objDiv!.scrollHeight;
  }

  selectMessage(message: Message): void {
    this.selectedMessages.push(message);
  }

  deleteMessages() {
    this.chatService.deleteMessages(this.selectedMessages.map(x => x.id)).toPromise()
      .then(
        _ => {
          this.scrollToBottom();
          this.chatService.getMessages(this.dialogId).toPromise()
            .then((data: Message[]) => {
              this.chatService.getCount(this.userId);
              this.messages = data;
              this.cd.detectChanges();
            });
        }
      );
  }

  ngOnDestroy(): void {
    this.hub.connection.off("ReceiveMessage");
    this.hub.connection.on("ReceiveMessage", (message: Message) => {
      if (message.userId != this.userId) {
        this.chatService.getCount(this.userId);
      }
    });
  }
}
