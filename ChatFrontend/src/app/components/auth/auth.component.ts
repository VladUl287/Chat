import { Component, ChangeDetectionStrategy, OnInit } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from 'src/app/services/auth.service';
import { HubService } from 'src/app/services/hub.service';

@Component({
  selector: 'app-auth',
  templateUrl: './auth.component.html',
  styleUrls: ['./auth.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AuthComponent implements OnInit {
  isLogin: boolean = true;
  image: File | null = null;
  loginForm = new FormGroup({
    email: new FormControl(''),
    password: new FormControl('')
  });

  registerForm = new FormGroup({
    email: new FormControl(''),
    login: new FormControl(''),
    password: new FormControl('')
  });

  constructor(private authService: AuthService, private hub: HubService, private router: Router) {}
  
  ngOnInit(): void {
    if(localStorage.getItem('token') !== null) {
      this.router.navigateByUrl('');
    }
  }

  login(): void {
    this.authService.login(this.loginForm).toPromise()
      .then((data: any) => {
          localStorage.setItem('token', data.token);
          this.hub.startConnection();
          this.router.navigateByUrl('');
        }
      );
  }

  register(): void {
    this.authService.register(this.registerForm, this.image!).toPromise()
      .then(() => this.registerForm.reset());
  }

  tooggle(): void {
    this.isLogin = !this.isLogin;
  }

  handleFileInput(event: any): void {
    let image = document.querySelector('.file-name')!;
    if(event.target.files[0]) {
      this.image = event.target.files[0];
      image.innerHTML = this.image!.name.substring(0, 20) + "...";
    } else {
      image.innerHTML = '';
    }
  }
}
