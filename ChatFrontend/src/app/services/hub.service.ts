import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { HttpError, HubConnection, HubConnectionBuilder } from "@aspnet/signalr";
import { BehaviorSubject } from 'rxjs';
import { environment } from 'src/environments/environment';
import { MessageModel } from '../models/messageModel';

@Injectable({
  providedIn: 'root'
})
export class HubService {
  private static hubConnection: HubConnection | undefined;
  private static readonly apiUrl: string = environment.apiUrl;
  private readonly isConnect: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
  
  constructor(private router: Router) {
    this.startConnection();
  }

  get connection(): HubConnection {
    if(!this.isConnect.value) {
      this.startConnection();
    }
    return HubService.hubConnection!;
  }

  public startConnection(): void {
    let token = localStorage.getItem('token');
    if (token) {
      HubService.hubConnection = new HubConnectionBuilder()
        .withUrl(HubService.apiUrl + '/chat', { accessTokenFactory: () => token! })
        .build();
        
      HubService.hubConnection
        .start()
        .then(() => { this.isConnect.next(true); })
        .catch((err: HttpError) => {
          if (err.statusCode === 401) {
            localStorage.removeItem('token');
            this.router.navigateByUrl('/auth');
          }
          else {
             setInterval(() => {
              this.startConnection();
             }, 5000);
          }
        });
    }
    else {
      this.stopConnection();
      this.router.navigateByUrl('auth');
    }
  }

  public stopConnection(): void {
    HubService.hubConnection?.stop();
    this.isConnect.next(false);
  }

  sendMessage(message: MessageModel): void {
    if (this.isConnect.value) {
      this.connection.invoke("SendMessage", message)
    }
  }

  addFriend(id: number): Promise<any> | null {
    if (this.isConnect.value) {
     return this.connection.invoke("AddFriend", id);
    }
    return null;
  }

  checkDialog(dialogId: number): void {
    if (this.isConnect.value) {
      this.connection.invoke("CheckDialog", dialogId);
    }
  }
}
