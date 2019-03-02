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

import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { Router } from '@angular/router';
import { VdlIconRegistry } from 'vdl-angular';
import { GarbService } from '../garb.service';
import { AuthService } from '../auth.service';
import { LocalStorageService } from '../local-storage.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})

export class LoginComponent implements OnInit {
  title: string = 'Assisted Review Portal';
  subtitle: string = '1.0';
  submitButtonLabel: string = 'Sign In';
  submitButtonLoading: boolean = false;

  username: string;
  password: string;
  errorMessage: string;


  constructor(vdlIconRegistry: VdlIconRegistry,
    public garbService: GarbService,
    private authService: AuthService,
    private localstorageservice: LocalStorageService,
    private _router: Router) {
    vdlIconRegistry.registerFontClassAlias('fontawesome', 'fa');
  }
    
  ngOnInit(): void {
  }

  _submitForm(event: Event) {
    event.preventDefault();
    event.stopPropagation();
    this.submitButtonLabel = "Signing In...";
    this.submitButtonLoading = true;
    this.Login();
  }

  Login(): void {
    this.authService.login(this.username, this.password).subscribe(
      data => {
        this.localstorageservice.SetAuthorizationData(data);
        this.authService.showMenu = true;
        this._router.navigate(['/screenshot']);
      },
      error => {
        this.errorMessage = "Invalid Credentials!"
        this.submitButtonLabel = 'Sign In';
        this.submitButtonLoading = false;        
      }
    );
  }
}