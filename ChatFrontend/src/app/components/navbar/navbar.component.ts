import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';
import { Message } from 'src/app/models/message';
import { UserToken } from 'src/app/models/userToken';
import { ChatService } from 'src/app/services/chat.service';
import { HubService } from 'src/app/services/hub.service';
import { TokenService } from 'src/app/services/token.service';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class NavbarComponent implements OnInit {
  email: string = '';
  count: number = 0;

  constructor(
    private router: Router,
    private hub: HubService,
    private chat: ChatService,
    private tokenService: TokenService,
    private cd: ChangeDetectorRef) { }

  ngOnInit(): void {
    let token: UserToken = this.tokenService.token;
    let userId: number = token.id;
    this.email = token.email;

    this.hub.connection.on("ReceiveMessage", (message: Message) => {
      if (message.userId != userId) {
        this.chat.getCount(userId);
      }
    });
    this.hub.connection.on("ReceiveCountDialogs", (count: number) => {
      this.count = count;
      this.cd.detectChanges();
    });
    this.chat.getCount(userId);

    let btn = document.querySelector('#menu-btn');
    let navBar = document.querySelector('.nav-bar');
    btn?.addEventListener('click', () => {
      navBar?.classList.toggle('active');
    });
  }

  logout() {
    this.tokenService.remove();
    this.router.navigateByUrl("auth");
  }
}
