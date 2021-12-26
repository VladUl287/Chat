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
    return this.http.get<number>(this.apiUrl + `/api/chat/count/${userId}`);
  }

  getDialog(fromId: number, toId: number): Observable<Message[]> {
    return this.http.get<Message[]>(this.apiUrl + `/api/chat/${fromId}/${toId}`);
  }

  getDialogs(userId: number): Observable<Dialog[]> {
    return this.http.get<Dialog[]>(this.apiUrl + `/api/chat/${userId}`);
  }
}