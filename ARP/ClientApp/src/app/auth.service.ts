/*
Copyright (c) 2019 Veritas Technologies LLC

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { JwtHelperService } from '@auth0/angular-jwt';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { GarbService } from './garb.service';
import { TokenParams } from './token-params';

const helper = new JwtHelperService();

@Injectable()
export class AuthService {
  public showMenu: boolean = false;
  private apiUrl: string;


  constructor(
    private _router: Router,
    private httpclient: HttpClient,
    @Inject('API_URL') private originUrl: string) {
    this.apiUrl = originUrl + "/api/v1.0/token";
  }

  login(Username: string, Password: string): Observable<TokenParams> {
    let headersForTokenAPI = new HttpHeaders({ 'Content-Type': 'application/json' });
    let data = '{ "username": "' + Username + '", "password": "' + Password + '" }';

    return this.httpclient.post<TokenParams>(this.apiUrl, data, { headers: headersForTokenAPI });
  }

  checkAuthentication() {
    try {
      if (!localStorage.getItem("Authorization") || helper.isTokenExpired(localStorage.getItem("Authorization"))) {
        this.Logout();
      }
      else {
        this.showMenu = true;
      }
    }
    catch (e) {
      this.Logout();
    }
  }

  Logout() {
    localStorage.removeItem("Authorization");
    this.showMenu = false;
    this._router.navigate(['/login']);
  }
}
