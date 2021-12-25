import { Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { Router } from '@angular/router';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {

  constructor(private router: Router) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    if(localStorage.getItem('token') != null) {
      const cloneReq = request.clone({
        headers: request.headers.set('Authorization', 'Bearer ' + localStorage.getItem('token'))
      });
      return next.handle(cloneReq).pipe(
        tap(
          succ => {},
          err => {
            if(err.status == 401) {
              localStorage.removeItem('token');
              this.router.navigateByUrl('/auth');
            }
          }
        )
      );
    }
    return next.handle(request);
  }
}
