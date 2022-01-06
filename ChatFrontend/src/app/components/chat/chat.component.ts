import { Component, OnInit, ChangeDetectionStrategy, OnDestroy } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { ReplaySubject, Subscription } from 'rxjs';
import { Dialog } from 'src/app/models/dialog';
import { Message } from 'src/app/models/message';
import { MessageModel } from 'src/app/models/messageModel';
import { User } from 'src/app/models/user';
import { ChatService } from 'src/app/services/chat.service';
import { HubService } from 'src/app/services/hub.service';
import { TokenService } from 'src/app/services/token.service';

@Component({
  selector: 'app-chat',
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChatComponent implements OnInit, OnDestroy {
  
  public messages$: ReplaySubject<Array<Message>> = new ReplaySubject<Array<Message>>();
  public selectedMessages: Array<number> = new Array<number>(); 
  public messages: Array<Message> = new Array<Message>();
  public userId: number = this.tokenService.token.id;
  public dialogView$: Promise<Dialog> | undefined;
  public users$: Promise<User[]> | undefined;
  public isDeleteMode: boolean = false; 
  public controlShow: boolean = false;
  public content: string = '';
  private dialogId: number = 0;
  private acceptScroll: boolean = true;
  private routeSub: Subscription | undefined;

  constructor(
    private hub: HubService,
    private chatService: ChatService,
    private tokenService: TokenService,
    private activatedRoute: ActivatedRoute,
    ) {}

  public ngOnInit(): void {
    this.routeSub = this.activatedRoute.params.subscribe(
      (data: Params) => {
        this.dialogId = +data.id;
        this.getMessages();
      });

      this.hub.connection.on("ReceiveMessage", (message: Message) => {
      this.acceptScroll = true;
      this.messages.push(message);
      this.messages$.next(this.messages);
      if (this.userId != message.userId) {
        this.hub.checkDialog(this.dialogId);
        this.chatService.getCount(this.userId);
      }
    });

    this.dialogView$ = this.chatService.getDialogView(this.dialogId).toPromise();
  }

  private getMessages(): void {
    this.chatService.getMessages(this.dialogId).toPromise()
      .then((data: Message[]) => {
          this.messages = data;
          this.messages$.next(data);
          this.chatService.getCount(this.userId);
        });
  }

  public send(): void {
    if(this.content.length > 0) {
      let message = new MessageModel(this.content, this.userId, this.dialogId);
      this.hub.sendMessage(message);
      this.content = "";
    }
  }

  public dialogInfo(show: boolean): void {
    this.controlShow = show;
    if(this.controlShow && !this.users$) {
      this.users$ = this.chatService.getUsersDialog(this.dialogId).toPromise();
    }
  }

  public scrollToBottom(): void {
    if(this.acceptScroll) {
      this.acceptScroll = false;
      let objDiv = document.querySelector(".messages");
      if(objDiv) {
        objDiv.scrollTop = objDiv.scrollHeight;
      }
    }
  }

  public selectMessage(message: Message): void {
    if(this.isDeleteMode) {
      if(message.isSelected) {
        let index: number = this.selectedMessages.indexOf(message.id);
        if(index > -1) {
          this.selectedMessages.splice(index, 1);
          message.isSelected = false;
        }
        return;
      }
      message.isSelected = true;
      this.selectedMessages.push(message.id);
    }
  }

  public deleteMessages(): void {
    if(this.isDeleteMode && this.selectedMessages.length > 0) {
      this.isDeleteMode = false;
      this.chatService.deleteMessages(this.selectedMessages).toPromise()
        .then(_ => {
            this.getMessages();
            this.selectedMessages = [];
          });
    }
  }

  public toogleDeleteMode(enabled: boolean): void {
    this.isDeleteMode = enabled;
    if(!enabled) {
      this.messages.forEach(message => message.isSelected = false);
      this.selectedMessages = [];
    }
  }

  public ngOnDestroy(): void {
    this.routeSub?.unsubscribe();
    //??????
    this.hub.connection.off("ReceiveMessage");
    this.hub.connection.on("ReceiveMessage", (message: Message) => {
      if (message.userId != this.userId) {
        this.chatService.getCount(this.userId);
      }
    });
  }
}
