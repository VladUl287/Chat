import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { ReplaySubject } from 'rxjs';
import { Dialog } from 'src/app/models/dialog';
import { ChatService } from 'src/app/services/chat.service';
import { HubService } from 'src/app/services/hub.service';
import { TokenService } from 'src/app/services/token.service';

@Component({
  selector: 'app-dialogues',
  templateUrl: './dialogues.component.html',
  styleUrls: ['./dialogues.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DialoguesComponent implements OnInit {
  dialogues: ReplaySubject<Array<Dialog>> = new ReplaySubject<Array<Dialog>>();
  userId: number;

  constructor(
    tokenService: TokenService, 
    private hub: HubService, 
    private chat: ChatService) {
    this.userId = tokenService.token.id;
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
}
