import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Message } from 'src/app/models/message';
import { MessageModel } from 'src/app/models/messageModel';
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
  public messages: Array<Message> = new Array<Message>();
  public messageText: string = '';
  private toId: number = 0;
  public userId: number = 0;

  constructor(
    actRoute: ActivatedRoute,
    private hub: HubService,
    private chatService: ChatService,
    private tokenService: TokenService,
    private cd: ChangeDetectorRef) {
    actRoute.params.subscribe(
      data => {
        this.toId = +data.id;
      }
    );
  }

  ngOnInit(): void {
    this.hub.connection.on("ReceiveMessage", (message: Message) => {
      this.messages.push(message);

      if (this.userId != message.userId) {

        this.chatService.getCount(this.userId);
        // this.hub.checkDialog(this.userId, this.toId);
        // this.hub.countDialogs(this.userId);
      }
      this.cd.detectChanges();
    });

    this.userId = this.tokenService.token.id;
    this.chatService.getDialog(this.userId, this.toId).toPromise()
      .then((data: Message[]) => {
        // this.hub.countDialogs(this.userId);
        this.chatService.getCount(this.userId);
        this.messages = data;
        this.cd.detectChanges();
      });

  }

  send(): void {
    let message: MessageModel = new MessageModel(this.messageText, this.userId, 1);
    this.hub.sendMessage(message);
  }

  scrollToBottom(): void {
    let objDiv = document.querySelector(".message-zone");
    objDiv!.scrollTop = objDiv!.scrollHeight;
  }

  ngOnDestroy(): void {
    this.hub.connection.off("ReceiveMessage");
    this.hub.connection.on("ReceiveMessage", (message: Message) => {
      if (message.userId != this.userId) {
        // this.hub.countDialogs(this.userId);
        this.chatService.getCount(this.userId);
      }
    });
  }
}