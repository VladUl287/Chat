import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/internal/Observable';
import { environment } from 'src/environments/environment';
import { User } from '../models/user';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  getUsers(id: number): Observable<User[]> {
    return this.http.get<User[]>(`${this.apiUrl}/api/user/all/${id}`);
  }

  getUser(id: number): Observable<User> {
    return this.http.get<User>(`${this.apiUrl}/api/user/${id}`);
  }

  accept(id: number, fromId: number): Observable<null> {
    return this.http.patch<null>(this.apiUrl + '/api/friend', { id, fromId });
  }

  getFriends(id: number): Observable<User[]> {
    return this.http.get<User[]>(`${this.apiUrl}/api/friend/${id}`);
  }

  getOutgoing(id: number): Observable<User[]> {
    return this.http.get<User[]>(`${this.apiUrl}/api/friend/outgoing/${id}`);
  }

  getIncoming(id: number): Observable<User[]> {
    return this.http.get<User[]>(`${this.apiUrl}/api/friend/incoming/${id}`);
  }

  search(login: string): Observable<User[]> {
    return this.http.get<User[]>(`${this.apiUrl}/api/user/search/${login}`);
  }
}