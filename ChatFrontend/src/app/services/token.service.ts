import { Injectable } from '@angular/core';
import { UserToken } from '../models/userToken';

@Injectable({
  providedIn: 'root'
})
export class TokenService {
  private static user: UserToken | undefined; 

  constructor() {
    this.token;
  }

  get token(): UserToken {
    if (!TokenService.user) {
      let token = localStorage.getItem('token');
      if (token !== null) {
        let parseToken = JSON.parse(window.atob(token.split('.')[1]));
        TokenService.user = new UserToken(+parseToken.unique_name, parseToken.email, parseToken.role);
      }
    }
    return TokenService.user!;
  }

  remove() {
    localStorage.removeItem('token');
  }
}