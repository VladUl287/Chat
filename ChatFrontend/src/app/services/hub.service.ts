import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { HttpError, HubConnection, HubConnectionBuilder, LogLevel } from "@aspnet/signalr";
import { BehaviorSubject } from 'rxjs';
import { environment } from 'src/environments/environment';
import { MessageModel } from '../models/messageModel';

@Injectable({
  providedIn: 'root'
})
export class HubService {
  public static hubConnection: HubConnection;
  public isConnect: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
  private static readonly apiUrl: string = environment.apiUrl;

  constructor(private router: Router) {
    let token = localStorage.getItem('token');
    if (token !== null) {
      HubService.hubConnection = new HubConnectionBuilder()
        .configureLogging(LogLevel.Debug)
        .withUrl(HubService.apiUrl + '/chat', { accessTokenFactory: () => token! })
        .build();
      this.startConnection();
    }
    else {
      router.navigateByUrl('auth');
    }
  }

  get connection(): HubConnection {
    return HubService.hubConnection;
  }

  private startConnection(): void {
    HubService.hubConnection
      .start()
      .then(() => { this.isConnect.next(true); })
      .catch((err: HttpError) => {
        if (err.statusCode === 401) {
          localStorage.removeItem('token');
          this.router.navigateByUrl('/auth');
        }
        else {
           //setInterval(() => {
           //  this.startConnection();
           //}, 5000);
        }
      });
  }

  sendMessage(message: MessageModel) {
    // this.connection.invoke("SendMessage", message);
    if (this.isConnect.value) {
      this.connection.invoke("SendMessage", message)
    }
    else {
      this.isConnect.subscribe(
        (data: boolean) => {
          if (data) {
            this.connection.invoke("SendMessage", message);
            this.isConnect.complete();
          }
        });
    }
  }

  countDialogs(userId: number): void {
    if (this.isConnect.value) {
      this.connection.invoke("CountDialogs", userId);
    }
    else {
      this.isConnect.subscribe(
        (data: boolean) => {
          if (data) {
            this.connection.invoke("CountDialogs", userId);
            this.isConnect.complete();
          }
        });
    }
  }

  addFriend(id: number): void {
    if (this.isConnect.value) {
      this.connection.invoke("AddFriend", id);
    }
    else {
      this.isConnect.subscribe(
        (data: boolean) => {
          if (data) {
            this.connection.invoke("AddFriend", id);
            this.isConnect.complete();
          }
        });
    }
  }

  checkDialog(dialogId: number): void {
    if (this.isConnect.value) {
      this.connection.invoke("CheckDialog", dialogId);
    }
    else {
      this.isConnect.subscribe(
        (data: boolean) => {
          if (data) {
            this.connection.invoke("CheckDialog", dialogId);
            this.isConnect.complete();
          }
        });
    }
  }
}
