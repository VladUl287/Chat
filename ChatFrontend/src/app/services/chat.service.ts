import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { Dialog } from '../models/dialog';
import { Message } from '../models/message';

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  private readonly apiUrl: string = environment.apiUrl;
  
  constructor(private http: HttpClient) {}

  getCount(userId: number): Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/api/chat/count/${userId}`);
  }

  getDialog(userId: number, toUserId: number): Observable<Dialog> {
    return this.http.get<Dialog>(`${this.apiUrl}/api/chat/dialog/${userId}/${toUserId}`);
  }

  getMessages(dialogId: number): Observable<Message[]> {
    return this.http.get<Message[]>(`${this.apiUrl}/api/chat/messages/${dialogId}`);
  }

  getDialogs(userId: number): Observable<Dialog[]> {
    return this.http.get<Dialog[]>(`${this.apiUrl}/api/chat/dialogs/${userId}`);
  }

  checkDialog(dialogId: number): Observable<null> {
    return this.http.get<null>(`${this.apiUrl}/api/chat/dialog/check/${dialogId}`);
  }

  createDialog(dialog: any): Observable<null> {
    return this.http.post<null>(`${this.apiUrl}/api/chat/create/dialog`, dialog);
  }
}
