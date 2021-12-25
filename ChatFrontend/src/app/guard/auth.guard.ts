import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  constructor(private router: Router) {}
  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): boolean {
      if(localStorage.getItem('token') != null) {
        let roles = route.data['roles'] as Array<string>;
        if(roles) {
          let match = this.roleMatch(roles);
          if(match) {
            return true;
          }
          this.router.navigateByUrl('/not-found');
          return false;
        }
        return true;
      }
      this.router.navigateByUrl('/auth');
      return false;
  }

  roleMatch(allowedRoles: Array<string>): boolean {
    let isMatch = false;
    let token = localStorage.getItem('token');
    if(token != null) {
      let userRole: string = JSON.parse(window.atob(token.split('.')[1])).role;
      allowedRoles.forEach(elem => {
        if(userRole == elem) {
          isMatch = true;
          return;
        }
      });
    }
    return isMatch;
  }
}
