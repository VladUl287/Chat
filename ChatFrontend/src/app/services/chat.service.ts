import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, ReplaySubject } from 'rxjs';
import { environment } from 'src/environments/environment';
import { Dialog } from '../models/dialog';
import { Message } from '../models/message';
import { User } from '../models/user';

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  private readonly apiUrl: string = environment.apiUrl;
  public readonly countDialogs: ReplaySubject<number> = new ReplaySubject<number>(0);

  constructor(private http: HttpClient) {}

  getCount(userId: number): void {
    this.http.get<number>(`${this.apiUrl}/api/chat/dialog/count/${userId}`).toPromise()
      .then(
        (data: number) => {
          this.countDialogs.next(data);
        }
      );
  }

  getDialog(userId: number, toUserId: number): Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/api/chat/dialog/${userId}/${toUserId}`);
  }

  getDialogView(dialogId: number): Observable<Dialog> {
    return this.http.get<Dialog>(`${this.apiUrl}/api/chat/dialog/${dialogId}`);
  }

  getUsersDialog(dialogId: number): Observable<User[]> {
    return this.http.get<User[]>(`${this.apiUrl}/api/chat/dialog/users/${dialogId}`);
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

  createDialog(formData: FormData): Observable<null> {
    return this.http.post<null>(`${this.apiUrl}/api/chat/create/dialog`, formData);
  }

  deleteDialog(id: number): Observable<null> {
    console.log(`${this.apiUrl}/api/chat/dialog/${id}`);
    return this.http.delete<null>(`${this.apiUrl}/api/chat/dialog/${id}`);
  }

  deleteMessages(arrId: number[]): Observable<null> {
    const options = {
       headers: new HttpHeaders({
          "Content-Type": "application/json;charset=utf-8"
       }),
       body: JSON.stringify(arrId)
    }
    return this.http.delete<null>(`${this.apiUrl}/api/chat/messages`, options);
  }
}
