import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl: string = environment.apiUrl;

  constructor(private http: HttpClient) {}

  login(form: FormGroup) {
    return this.http.post(this.apiUrl + '/api/auth/login', this.getFormData(form));
  }

  register(form: FormGroup, image: File) {
    let formData: FormData = this.getFormData(form);
    formData.append('imageFile', image, image.name);
    return this.http.post(this.apiUrl + '/api/auth/register', formData);
  }

  private getFormData(form: FormGroup): FormData {
    const formData: FormData = new FormData();
    Object.keys(form.controls).forEach((key: string) => {
      formData.append(key, form.controls[key].value);
    });
    return formData;
  }
}
